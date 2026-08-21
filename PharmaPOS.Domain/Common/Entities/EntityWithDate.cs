namespace PharmaPOS.Domain.Common.Entities;

public class EntityWithDate : Entity
{
	public DateTime CreatedDate { get; protected init; }
}