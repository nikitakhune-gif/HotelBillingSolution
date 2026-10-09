using HotelBilling.Application.Interfaces;
using HotelBilling.Application.DTOs.Booking;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using HotelBilling.Application.Interfaces;
using System;
using System.Linq;
using HotelBilling.Domain.Interfaces;
using HotelBilling.Domain.Entities;
using System.Collections.Generic;

namespace HotelBilling.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IBookingService _bookingService;
        private readonly ICustomerService _customerService;
        private readonly IRoomService _roomService;
        private readonly IHousekeepingService _housekeepingService;
        private readonly IUserService _userService;

        public ChatService(IBookingService bookingService,
            ICustomerService customerService,
            IRoomService roomService,
            IHousekeepingService housekeepingService,
            IUserService userService)
        {
            _bookingService = bookingService;
            _customerService = customerService;
            _roomService = roomService;
            _housekeepingService = housekeepingService;
            _userService = userService;
        }

        public async Task<object> ProcessMessageAsync(string message, string userName, string role)
        {
            if (string.IsNullOrWhiteSpace(message)) return new { text = "Please ask a question." };

            var msg = message.ToLowerInvariant().Trim();

            // Intent detection (simple rules)
            if (msg.Contains("customer") || msg.Contains("customers") || msg.Contains("guests"))
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var list = customers.Take(10).Select(c => new { id = c.Id, name = (c.FirstName + " " + c.LastName).Trim(), email = c.Email, phone = c.MobileNumber });
                return new { text = $"Found {customers.Count()} customers.", table = list };
            }

            if (msg.Contains("today") && (msg.Contains("booking") || msg.Contains("bookings") || msg.Contains("check-in") || msg.Contains("checkin") || msg.Contains("check ins")))
            {
                var summary = await _bookingService.GetBookingSummaryAsync();
                return new { text = $"Today's bookings: {summary.TodaysBookings}, Check-ins: {summary.CheckInsToday}, Check-outs: {summary.CheckOutsToday}", data = summary };
            }

            if (msg.Contains("available") && msg.Contains("room"))
            {
                var today = DateTime.UtcNow.Date;
                var next = today.AddDays(1);
                var rooms = await _bookingService.GetAvailableRoomsAsync(today, next, string.Empty, null);
                var list = rooms.Take(10).Select(r => new { id = r.Id, number = r.RoomNumber, type = r.RoomType, price = r.PricePerNight, availability = r.Availability });
                return new { text = $"Found {rooms.Length} available rooms.", table = list };
            }

            // Booking by reference BK-
            var bkMatch = Regex.Match(message, "BK-\\d{14}", RegexOptions.IgnoreCase);
            if (bkMatch.Success)
            {
                var bk = bkMatch.Value;
                // try to find booking by BookingNumber
                // There's no direct method, so query paged list for matching number
                var filter = new HotelBilling.Application.DTOs.Booking.BookingFilterDto { Page = 1, PageSize = 1, BookingNumber = bk };
                var paged = await _bookingService.GetPagedBookingsAsync(filter);
                var item = paged.Items.FirstOrDefault();
                if (item != null) return new { text = $"Booking {bk} found.", booking = item };
                return new { text = $"Booking {bk} not found." };
            }

            // Booking id or id phrase
            var idMatch = Regex.Match(message, "\\bBK-(\\d{14})\\b", RegexOptions.IgnoreCase);

            // Search by booking id numeric or guest name
            if (msg.StartsWith("details of") || msg.StartsWith("details") || msg.StartsWith("show details") || msg.StartsWith("details for"))
            {
                var q = message.Replace("details of", "").Replace("details for", "").Replace("show details", "").Trim();
                // If contains BK-
                var m = Regex.Match(q, "BK-\\d{14}", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var filter = new HotelBilling.Application.DTOs.Booking.BookingFilterDto { Page = 1, PageSize = 1, BookingNumber = m.Value };
                    var paged = await _bookingService.GetPagedBookingsAsync(filter);
                    var item = paged.Items.FirstOrDefault();
                    if (item != null) return new { text = $"Booking {m.Value}:", booking = item };
                    return new { text = $"Booking {m.Value} not found." };
                }

                // Maybe guest name
                var filter2 = new HotelBilling.Application.DTOs.Booking.BookingFilterDto { Page = 1, PageSize = 5, GuestName = q };
                var paged2 = await _bookingService.GetPagedBookingsAsync(filter2);
                if (paged2.TotalCount > 0)
                    return new { text = $"Found {paged2.TotalCount} bookings for '{q}'", list = paged2.Items };

                return new { text = "No matching booking found." };
            }

            if (msg.Contains("who checks out") || msg.Contains("checks out tomorrow") || msg.Contains("checkout tomorrow") || msg.Contains("who checks out tomorrow"))
            {
                var tomorrow = DateTime.UtcNow.Date.AddDays(1);
                var all = await _bookingService.GetPagedBookingsAsync(new HotelBilling.Application.DTOs.Booking.BookingFilterDto { Page = 1, PageSize = 1000 });
                var items = all.Items.Where(b => b.CheckOutDate.Date == tomorrow).Select(b => new { b.BookingNumber, b.GuestName, b.RoomNumber, b.CheckOutDate }).ToList();
                return new { text = $"{items.Count} guests checking out tomorrow.", table = items };
            }

            // Fallback
            return new { text = "I couldn't understand that. Try: 'customers', 'show today's bookings', 'which rooms are available?', 'details of BK-20260818115532', 'who checks out tomorrow?'" };
        }
    }
}
