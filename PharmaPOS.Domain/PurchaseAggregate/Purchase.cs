using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Domain.PurchaseAggregate;

public class Purchase : OrderDetails
{
	private readonly List<Batch> _batches = [];

	// EF Core parameterless constructor
	private Purchase() { }

	public Purchase(PurchaseDetails details)
	{
		Update(details);
	}

	public int SupplierId { get; private set; }
	public Supplier Supplier { get; private set; } = null!;

	public IReadOnlyCollection<Batch> Batches => _batches.AsReadOnly();

	public void Update(PurchaseDetails details)
	{
		if (details.SupplierId <= 0)
			throw new ArgumentException("Valid Supplier is required.", nameof(details.SupplierId));
		if (details.SubTotal < 0)
			throw new ArgumentException("Subtotal cannot be negative.", nameof(details.SubTotal));
		if (details.Tax < 0)
			throw new ArgumentException("Tax cannot be negative.", nameof(details.Tax));
		if (details.Discount < 0)
			throw new ArgumentException("Discount value cannot be negative.", nameof(details.Discount));
		if (details.Amount < 0)
			throw new ArgumentException("Amount cannot be negative.", nameof(details.Amount));

		SupplierId = details.SupplierId;
		Date = details.Date;
		SubTotal = details.SubTotal;
		Tax = details.Tax;
		Discount = details.Discount;
		Amount = details.Amount;
		Note = details.Note;
	}

	public void AddBatch(Batch batch)
	{
		_batches.Add(batch);
	}

	public void RemoveBatch(Batch batch)
	{
		_batches.Remove(batch);
	}

	public void ClearBatches()
	{
		_batches.Clear();
	}
}
