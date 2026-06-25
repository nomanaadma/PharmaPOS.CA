using System;
using System.Collections.Generic;

namespace PharmaPOS_MVC.Domain.Entities;

public class MedicinePurchase
{
	private readonly List<MedicineBatch> _batches = new();

	// EF Core parameterless constructor
	private MedicinePurchase() { }

	public MedicinePurchase(int supplierId, DateTime date, decimal total, decimal tax, decimal discountValue, decimal amount, string? note)
	{
		Update(supplierId, date, total, tax, discountValue, amount, note);
		CreatedDate = DateTime.UtcNow;
	}

	public int Id { get; private set; }
	public int SupplierId { get; private set; }
	public Supplier Supplier { get; private set; } = null!;
	public DateTime Date { get; private set; }
	public decimal Total { get; private set; }
	public decimal Tax { get; private set; }
	public decimal DiscountValue { get; private set; }
	public decimal Amount { get; private set; }
	public string? Note { get; private set; }
	public DateTime CreatedDate { get; private set; }

	public IReadOnlyCollection<MedicineBatch> Batches => _batches.AsReadOnly();

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
		Total = total;
		Tax = tax;
		DiscountValue = discountValue;
		Amount = amount;
		Note = note;
	}

	public void AddBatch(MedicineBatch batch)
	{
		_batches.Add(batch);
	}

	public void RemoveBatch(MedicineBatch batch)
	{
		_batches.Remove(batch);
	}

	public void ClearBatches()
	{
		_batches.Clear();
	}
}
