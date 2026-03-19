using FluentValidation;
using HotelBilling.Application.DTOs.Request;

namespace HotelBilling.Application.Validators
{
    public class CustomerValidator : AbstractValidator<CreateCustomerRequest>
    {
        public CustomerValidator()
        {
            // Name
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            // Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            // Phone
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^[0-9]{10}$").WithMessage("Phone must be 10 digits");
        }
    }
}