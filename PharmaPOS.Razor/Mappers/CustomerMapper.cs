using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Mappers;

public static class CustomerMapper
{
    public static CustomerViewModel MapToCustomer(Customer c)
    {
        return new CustomerViewModel
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email ?? string.Empty,
            Mobile = c.Mobile ?? string.Empty,
            Gender = c.Gender,
            Status = c.Status,
            CreatedDate = c.CreatedDate
        };
    }

    public static IEnumerable<CustomerViewModel> MapToCustomers(IEnumerable<Customer> customers)
    {
        return customers.Select(MapToCustomer);
    }
    
    public static IEnumerable<CustomerBillViewModel> MapToCustomerBills(
	    IEnumerable<Bill> bills)
    {
	    return bills.Select(b => new CustomerBillViewModel
	    {
		    Id = b.Id,
		    Amount = b.Amount,
		    BillDate = b.Date
	    });
    }
}
