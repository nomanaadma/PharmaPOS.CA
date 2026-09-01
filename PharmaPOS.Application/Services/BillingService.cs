using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Data;
using PharmaPOS.Application.Helpers;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Application.Services;

public class BillingService(
	ApplicationDbContext context,
	CustomerService customerService,
	MedicineService medicineService,
	BatchService batchService,
	ILogger<BillingService> logger) : IApplicationService
{

	/// <summary>
	/// Gets filtered bills for DataTables.
	/// </summary>
	public async Task<DataTableResponse<Bill>> GetFilteredBillsAsync(DataTableRequest request, DataTableColumnMap<Bill> columns, CancellationToken ct = default)
	{
		var query = context.Bills.AsQueryable();
		return await DataTableHelper.FilterAsync(query, request, columns, ct);
	}

	/// <summary>
	/// Creates a new bill with items, adjusting inventory (batch sold quantities) within a transaction.
	/// </summary>
	public async Task<ErrorOr<int>> CreateBillAsync(BillingRequest request, CancellationToken ct = default)
	{
		if (request.Items.Count == 0)
			return BillErrors.MissingItems;

		var customerRequest = CreateCustomerRequest(request);
		
		var customerResult = await customerService.GetOrCreateCustomerAsync(customerRequest, ct);
		if (customerResult.IsError)
			return customerResult.Errors;

		var billDetails = new BillDetails(
			customerResult.Value,
			request.Name,
			request.Email,
			request.Mobile,
			request.Method,
			request.Date,
			request.SubTotal,
			request.Tax,
			request.Discount,
			request.Amount,
			request.Note
		);

		var billResult = Bill.Create(billDetails);
		if (billResult.IsError)
			return billResult.Errors;

		var bill = billResult.Value;

		// Pre-fetch all needed batches in a single query (fixes N+1)
		var batchIds = request.Items.Select(i => i.Batch).Distinct().ToList();
		var batches = await batchService.GetBatchByIdsAsync(batchIds, ct);

		foreach (var item in request.Items)
		{
			var pricingResult = PricingHelper.CreatePricingDetails(item);
			if (pricingResult.IsError) return pricingResult.Errors;

			var billItemDetails = new BillItemDetails(item.MedicineId, item.Batch, pricingResult.Value);
			var billItemResult = BillItem.Create(billItemDetails);

			if (billItemResult.IsError)
				return billItemResult.Errors;

			// Use aggregate root method
			bill.AddItem(billItemResult.Value);

			if (!batches.TryGetValue(item.Batch, out var batch))
				return BillErrors.BatchNotFound(item.Batch, item.MedicineId);

			// Validate stock availability
			var available = batch.Pricing.Qty - batch.Sold;
			if (item.Qty > available)
				return BillErrors.InsufficientStock(batch.BatchName, available, item.Qty);

			batch.UpdateSold(batch.Sold + item.Qty);
		}

		context.Bills.Add(bill);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Bill {BillId} created with {ItemCount} items", bill.Id, request.Items.Count);
		return bill.Id;
	}

	private static CustomerRequest CreateCustomerRequest(BillingRequest billingRequest)
	{
		
		var nameParts = (billingRequest.Name).Split(' ', StringSplitOptions.RemoveEmptyEntries);

		// Default to empty strings if the array is empty
		var lastName = nameParts.Length > 0 ? nameParts[^1] : string.Empty;

		var firstName = nameParts.Length > 1 
			? string.Join(" ", nameParts[..^1]) 
			: lastName;
		
		return new CustomerRequest(
			Id: billingRequest.CustomerId,
			FirstName: firstName,
			LastName: lastName,
			Email: billingRequest.Email,
			Mobile: billingRequest.Mobile,
			Gender: Gender.NotSpecified,
			Status: Status.Active
		);
		
	}

	/// <summary>
	/// Updates an existing bill: reverts old inventory, clears old items, adds new items, adjusts inventory.
	/// </summary>
	public async Task<ErrorOr<int>> UpdateBillAsync(int id, BillingRequest request, CancellationToken ct = default)
	{
		if (request.Items.Count == 0)
			return BillErrors.MissingItems;
		
		var bill = await context.Bills
			.Include(b => b.Items)
			.FirstOrDefaultAsync(b => b.Id == id, ct);

		if (bill == null)
			return BillErrors.NotFound(id);

		var customerId = request.CustomerId ?? bill.CustomerId;
		if (customerId == 0)
		{
			
			var customerRequest = CreateCustomerRequest(request);
			
			var customerResult = await customerService.GetOrCreateCustomerAsync(customerRequest, ct);
			if (customerResult.IsError)
				return customerResult.Errors;
			customerId = customerResult.Value;
		}

		var billDetails = new BillDetails(
			customerId,
			request.Name,
			request.Email,
			request.Mobile,
			request.Method,
			request.Date,
			request.SubTotal,
			request.Tax,
			request.Discount,
			request.Amount,
			request.Note
		);

		var updateResult = bill.Update(billDetails);
		if (updateResult.IsError)
			return updateResult.Errors;

		// Pre-fetch all batches for both old and new items in a single query
		var oldBatchIds = bill.Items.Select(i => i.BatchId).ToList();
		var newBatchIds = request.Items.Select(i => i.Batch).ToList();
		var allBatchIds = oldBatchIds.Union(newBatchIds).Distinct().ToList();

		
		var batches = await batchService.GetBatchByIdsAsync(allBatchIds, ct);

		// Revert sold quantities for old items
		foreach (var oldItem in bill.Items)
		{
			if (!batches.TryGetValue(oldItem.BatchId, out var batch))
				return BillErrors.BatchNotFound(oldItem.BatchId, oldItem.MedicineId);

			batch.UpdateSold(batch.Sold - oldItem.Pricing.Qty);
		}

		// Clear old items using aggregate root method
		bill.ClearItems();

		// Add new items and update sold quantities
		foreach (var item in request.Items)
		{
			var pricingResult = PricingHelper.CreatePricingDetails(item);
			if (pricingResult.IsError) return pricingResult.Errors;

			var billItemDetails = new BillItemDetails(item.MedicineId, item.Batch, pricingResult.Value);
			var billItemResult = BillItem.Create(billItemDetails);
			
			if (billItemResult.IsError)
				return billItemResult.Errors;

			// Use aggregate root method
			bill.AddItem(billItemResult.Value);
			
			if (!batches.TryGetValue(item.Batch, out var batch))
				return BillErrors.BatchNotFound(item.Batch, item.MedicineId);

			// Validate stock availability
			var available = batch.Pricing.Qty - batch.Sold;
			if (item.Qty > available)
				return BillErrors.InsufficientStock(batch.BatchName, available, item.Qty);

			batch.UpdateSold(batch.Sold + item.Qty);
		}

		await context.SaveChangesAsync(ct);

		logger.LogInformation("Bill {BillId} updated with {ItemCount} items", bill.Id, request.Items.Count);
		return bill.Id;
	}

	/// <summary>
	/// Deletes a bill and reverts inventory (batch sold quantities).
	/// </summary>
	public async Task<ErrorOr<Deleted>> DeleteBillAsync(int id, CancellationToken ct = default)
	{

		var bill = await context.Bills
			.Include(b => b.Items)
			.FirstOrDefaultAsync(b => b.Id == id, ct);

		if (bill == null)
			return BillErrors.NotFound(id);

		// Pre-fetch all batches in a single query
		var batchIds = bill.Items.Select(i => i.BatchId).Distinct().ToList();
		var batches = await batchService.GetBatchByIdsAsync(batchIds, ct);
		 
		// Revert sold quantities
		foreach (var item in bill.Items)
		{
			if (!batches.TryGetValue(item.BatchId, out var batch))
				return BillErrors.BatchNotFound(item.BatchId, item.MedicineId);

			batch.UpdateSold(batch.Sold - item.Pricing.Qty);
		}

		context.Bills.Remove(bill);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Bill {BillId} deleted, {ItemCount} items reverted", id, bill.Items.Count);
		return Result.Deleted;
	}

	/// <summary>
	/// Loads a bill with all related data for the Edit GET action.
	/// </summary>
	public async Task<ErrorOr<(
		Bill, 
		Dictionary<int, List<Batch>>,
		Dictionary<int, string>
		)>> GetBillForEditAsync(int id, CancellationToken ct = default)
	{

		var bill = await context.Bills
			.Include(b => b.Items)
			.AsSplitQuery()
			.FirstOrDefaultAsync(b => b.Id == id, ct);

		if (bill == null)
			return BillErrors.NotFound(id);

		var medicineIds = bill.Items.Select(i => i.MedicineId).Distinct().ToList();
		var itemBatchIds = bill.Items.Select(i => i.BatchId).Distinct().ToList();

		var activeBatches = await batchService.GetNonExpiredBatchesAsync(medicineIds, ct);

		// Also include the item's own batch even if expired/inactive
		var missingBatchIds = itemBatchIds.Except(activeBatches.Select(b => b.Id)).ToList();
		if (missingBatchIds.Count > 0)
		{
			var extraBatches = (await batchService.GetBatchByIdsAsync(missingBatchIds, ct)).Values.ToList();
			activeBatches.AddRange(extraBatches);
		}

		var batchesByMedicine = activeBatches
			.GroupBy(b => b.MedicineId)
			.ToDictionary(g => g.Key, g => g.ToList());
		
		var medicineNames = await medicineService.GetMedicineNamesByIds(medicineIds, ct);

		return (bill, batchesByMedicine, medicineNames);
	}

	/// <summary>
	/// Loads a bill with all related data for the Details (view) action.
	/// </summary>
	public async Task<ErrorOr<(Bill, Dictionary<int, Batch>, Dictionary<int, string>)>> GetBillAsync(int id, CancellationToken ct = default)
	{
		var bill = await context.Bills
			.Include(b => b.PaymentMethod)
			.Include(b => b.Items)
			.AsSplitQuery()
			.FirstOrDefaultAsync(b => b.Id == id, ct);

		if (bill == null)
			return BillErrors.NotFound(id);

		var batchIds = bill.Items.Select(i => i.BatchId).Distinct().ToList();
		var batchesById = await batchService.GetBatchByIdsAsync(batchIds, ct);
		
		var medicineIds = bill.Items.Select(b => b.MedicineId).Distinct().ToList();
		var medicineNames = await medicineService.GetMedicineNamesByIds(medicineIds, ct);
		
		return (bill, batchesById, medicineNames);
	}
	
	public async Task<List<Bill>> GetRecentBillsAsync(int customerId, string? email, CancellationToken ct = default)
	{
		return await context.Bills
			.Where(b => b.CustomerId == customerId || (email != null && b.Email == email))
			.OrderByDescending(b => b.Date)
			.Take(20)
			.ToListAsync(ct);
	}
	
}
