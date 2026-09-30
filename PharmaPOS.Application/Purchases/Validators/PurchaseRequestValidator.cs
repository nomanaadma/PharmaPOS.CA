using FluentValidation;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Purchases.Validators;

public class PurchaseRequestValidator : AbstractValidator<PurchaseRequest>
{
    public PurchaseRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.Supplier)
            .GreaterThan(0).WithMessage("Please select a valid supplier.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Purchase must have at least one item.");

        RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Tax).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
