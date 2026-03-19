using FluentValidation;
using HotelBilling.Application.DTOs.Request;

namespace HotelBilling.Application.Validators
{
    public class BillingValidator : AbstractValidator<CreateBillRequest>
    {
        public BillingValidator()
        {
            // CustomerId           
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer is required");

            // RoomId
            RuleFor(x => x.RoomId)
                .GreaterThan(0).WithMessage("Room is required");

            RuleFor(x => x.RoomCharge).GreaterThanOrEqualTo(0);
            RuleFor(x => x.FoodCharge).GreaterThanOrEqualTo(0);
            RuleFor(x => x.OtherCharges).GreaterThanOrEqualTo(0);
        }
    }
}