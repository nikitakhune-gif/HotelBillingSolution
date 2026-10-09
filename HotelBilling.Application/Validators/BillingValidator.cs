using FluentValidation;
using HotelBilling.Application.DTOs.Bill;

namespace HotelBilling.Application.Validators
{
    // Validate using the existing BillDto (Option A: align to existing DTOs)
    public class BillingValidator : AbstractValidator<BillDto>
    {
        public BillingValidator()
        {
            // CustomerId
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer is required");

            // RoomId
            RuleFor(x => x.RoomId)
                .GreaterThan(0).WithMessage("Room is required");

            // SubTotal and other numeric fields
            RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ServiceCharge).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
        }
    }
}
