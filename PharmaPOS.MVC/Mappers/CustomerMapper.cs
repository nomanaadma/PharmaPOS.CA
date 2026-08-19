using PharmaPOS.Domain.Entities;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Mappers;

public static class CustomerMapper
{
    public static CustomerViewModel MapToCustomer(Customer c)
    {
        return new CustomerViewModel
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Mobile = c.Mobile,
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
		    BillDate = b.BillDate
	    });
    }
}
