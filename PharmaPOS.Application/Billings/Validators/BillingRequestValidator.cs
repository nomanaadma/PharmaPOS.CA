using FluentValidation;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Billings.Validators;

public class BillingRequestValidator : AbstractValidator<BillingRequest>
{
    public BillingRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid Email Address")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Bill must have at least one item.");
            
        // Optional: Ensure amounts are non-negative structurally
        RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Tax).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
