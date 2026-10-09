using FluentValidation;
using HotelBilling.Application.DTOs;

namespace HotelBilling.Application.Validators
{
    public class PaymentValidator : AbstractValidator<PaymentCreateDto>
    {
        public PaymentValidator()
        {
            RuleFor(x => x.BillId).GreaterThan(0).WithMessage("Bill is required");
            RuleFor(x => x.PaymentDate).NotEmpty().WithMessage("Payment Date is required");
            RuleFor(x => x.Method).NotEmpty().WithMessage("Payment Method is required");
            // Amount/TotalAmount is computed server-side from Bill; client should not submit raw Total here.
        }
    }
}
