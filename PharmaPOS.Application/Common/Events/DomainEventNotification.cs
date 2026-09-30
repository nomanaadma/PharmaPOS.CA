using PharmaPOS.Domain.Common;

namespace PharmaPOS.Application.Common.Events;

public class DomainEventNotification<T>(T domainEvent) : IDomainEventNotification where T : IDomainEvent
{
    public IDomainEvent DomainEvent => domainEvent;
}
