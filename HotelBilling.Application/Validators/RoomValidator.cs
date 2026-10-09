using FluentValidation;
using HotelBilling.Application.DTOs.Room;

namespace HotelBilling.Application.Validators
{
    public class RoomValidator : AbstractValidator<CreateRoomDto>
    {
        public RoomValidator()
        {
            RuleFor(x => x.RoomNumber)
                .NotEmpty().WithMessage("Room number is required")
                .MaximumLength(10).WithMessage("Room number cannot exceed 10 characters");

            //RuleFor(x => x.Price)
            //    .GreaterThan(0).WithMessage("Price must be greater than zero");
        }
    }
}
