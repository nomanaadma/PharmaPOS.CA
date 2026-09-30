using MediatR;
using PharmaPOS.Application.Common.Events;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.BillingAggregate.Events;
using PharmaPOS.Shared.Enums;

namespace PharmaPOS.Application.Billings.Events;

public class BillDeletedEventHandler(
    ICustomerRepository customerRepo,
    IBillRepository billRepo,
    IUnitOfWork unitOfWork
) : INotificationHandler<DomainEventNotification<BillDeletedEvent>>
{
    public async Task Handle(DomainEventNotification<BillDeletedEvent> notification, CancellationToken ct)
    {
	    if (notification.DomainEvent is not BillDeletedEvent domainEvent) return;
        
        var customer = await customerRepo.GetByIdAsync(domainEvent.CustomerId, ct);
        
        if (customer == null) return;

        if (customer.Gender != Gender.NotSpecified) return; 
        
        var hasBills = await billRepo.HasBillsByCustomerAsync(domainEvent.CustomerId, ct);
        
        if (hasBills) return; 
        
        customerRepo.Remove(customer);
        await unitOfWork.SaveChangesAsync(ct);
        
    }
}
