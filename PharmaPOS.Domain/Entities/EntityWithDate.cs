namespace PharmaPOS.Domain.Entities;

public class EntityWithDate : Entity
{
	public DateTime CreatedDate { get; protected init; }
}