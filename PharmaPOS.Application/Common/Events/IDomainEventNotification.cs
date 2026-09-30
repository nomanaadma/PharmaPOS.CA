using MediatR;
using PharmaPOS.Domain.Common;

namespace PharmaPOS.Application.Common.Events;

public interface IDomainEventNotification : INotification
{
    IDomainEvent DomainEvent { get; }
}
