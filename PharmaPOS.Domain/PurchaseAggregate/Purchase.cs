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
