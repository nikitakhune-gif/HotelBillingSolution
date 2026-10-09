using System;
using System.Threading;
using System.Threading.Tasks;
using HotelBilling.Application.DTOs.Booking;
using HotelBilling.Application.DTOs.Booking;

namespace HotelBilling.Application.Interfaces
{
    public interface IBookingService
    {
        Task<HotelBilling.Application.DTOs.PagedResultDto<BookingListItemDto>> GetPagedBookingsAsync(BookingFilterDto filter, CancellationToken cancellationToken = default);

        Task<BookingSummaryDto> GetBookingSummaryAsync(CancellationToken cancellationToken = default);

        Task<HotelBilling.Application.DTOs.Room.RoomDto[]> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, string roomType, int? floor, int adults = 1, int children = 0, CancellationToken cancellationToken = default);

        Task<int> CreateBookingAsync(BookingCreateDto dto, CancellationToken cancellationToken = default);

        Task<(bool valid, decimal discountPercent, string message)> ApplyCouponAsync(string code, decimal roomTotal, CancellationToken cancellationToken = default);

        Task<BookingListItemDto?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
