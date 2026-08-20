namespace PharmaPOS.Domain.Entities;

public class Purchase : OrderDetails
{
	private readonly List<Batch> _batches = [];

	// EF Core parameterless constructor
	private Purchase() { }

	public Purchase(int supplierId, DateTime date, decimal total, decimal tax, decimal discountValue, decimal amount, string? note)
	{
		Update(supplierId, date, total, tax, discountValue, amount, note);
		CreatedDate = DateTime.UtcNow;
	}

	public int SupplierId { get; private set; }
	public Supplier Supplier { get; private set; } = null!;

	public IReadOnlyCollection<Batch> Batches => _batches.AsReadOnly();

	public void Update(int supplierId, DateTime date, decimal total, decimal tax, decimal discountValue, decimal amount, string? note)
	{
		if (supplierId <= 0)
			throw new ArgumentException("Valid Supplier is required.", nameof(supplierId));
		if (total < 0)
			throw new ArgumentException("Total cannot be negative.", nameof(total));
		if (tax < 0)
			throw new ArgumentException("Tax cannot be negative.", nameof(tax));
		if (discountValue < 0)
			throw new ArgumentException("Discount value cannot be negative.", nameof(discountValue));
		if (amount < 0)
			throw new ArgumentException("Amount cannot be negative.", nameof(amount));

		SupplierId = supplierId;
		Date = date;
		Subtotal = total;
		Tax = tax;
		DiscountValue = discountValue;
		Amount = amount;
		Note = note;
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
