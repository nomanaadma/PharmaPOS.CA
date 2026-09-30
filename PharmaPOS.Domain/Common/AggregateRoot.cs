namespace PharmaPOS.Domain.Common;

public abstract class AggregateRoot : Entity
{
	private readonly List<IDomainEvent> _domainEvents = [];
	protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
	
	public List<IDomainEvent> ClearDomainEvents()
	{
		var copy = _domainEvents.ToList();

		_domainEvents.Clear();

		return copy;
	}
}

