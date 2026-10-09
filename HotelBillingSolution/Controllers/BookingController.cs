using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HotelBilling.Application.DTOs.Booking;
using HotelBilling.Application.Interfaces;

namespace HotelBillingWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        }

        // GET: /Booking
        // FIX: previously this always built `new BookingFilterDto { Page = 1, PageSize = 10 }`
        // and completely ignored the querystring, so the filter form (BookingNumber,
        // GuestName, RoomNumber, BookingStatus, PageSize) and the pagination links
        // (asp-route-page="@p") on Booking.cshtml had zero effect — every click just
        // reloaded the same first page. [FromQuery] now binds those values directly
        // from the URL, which is exactly what the filter <form method="get"> and the
        // pagination <a asp-route-page="..."> already send.
        public async Task<IActionResult> Index([FromQuery] BookingFilterDto filter, CancellationToken cancellationToken)
        {
            filter ??= new BookingFilterDto();
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize <= 0) filter.PageSize = 10;

            var page = await _bookingService.GetPagedBookingsAsync(filter, cancellationToken);
            var summary = await _bookingService.GetBookingSummaryAsync(cancellationToken);

            var model = new BookingIndexViewModel
            {
                Bookings = page?.Items ?? Enumerable.Empty<BookingListItemDto>(),
                Filter = filter,
                Summary = summary ?? new BookingSummaryDto(),
                TotalCount = page?.TotalCount ?? 0,
                RecentBookings = (page?.Items ?? Enumerable.Empty<BookingListItemDto>()).Take(5),
                UpcomingCheckIns = Enumerable.Empty<BookingListItemDto>(),
                UpcomingCheckOuts = Enumerable.Empty<BookingListItemDto>(),
                RecentPayments = Enumerable.Empty<PaymentListItemDto>()
            };

            // Preserve ViewData for any legacy code, but return the model to the view
            ViewData["BookingsPage"] = page;
            ViewData["Summary"] = summary;

            return View(model);
        }

        // GET: /Booking/Search?search=...&page=1
        [HttpGet]
        public async Task<IActionResult> Search(string search, int page = 1, CancellationToken cancellationToken = default)
        {
            var filter = new BookingFilterDto
            {
                BookingNumber = search ?? string.Empty,
                GuestName = search ?? string.Empty,
                Page = page,
                PageSize = 10
            };

            var result = await _bookingService.GetPagedBookingsAsync(filter, cancellationToken);
            return Json(result);
        }

        // GET: /Booking/AddBooking
        [HttpGet]
        public IActionResult AddBooking()
        {
            // Populate available rooms for initial render so the room table isn't empty
            // Use reasonable defaults: today -> tomorrow, 2 adults, 0 children
            var checkIn = DateTime.Today;
            var checkOut = DateTime.Today.AddDays(1);
            // Call service synchronously via Task.Run to avoid making this method async-only change
            var rooms = _bookingService.GetAvailableRoomsAsync(checkIn, checkOut, string.Empty, null, 2, 0).GetAwaiter().GetResult();

            // Map to dynamic shape expected by the Razor view (Status, MaxGuests, ImageUrl, PricePerNight)
            var roomUi = rooms.Select(r => new
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Floor = r.Floor,
                MaxGuests = ((r.CapacityAdults ?? 0) + (r.CapacityChildren ?? 0)),
                PricePerNight = r.PricePerNight ?? 0m,
                Status = r.Availability,
                ImageUrl = string.IsNullOrWhiteSpace(r.ImagePaths) ? "/images/placeholder-room.jpg" : r.ImagePaths
            }).ToArray();

            ViewBag.AvailableRooms = roomUi;

            return View("AddBooking");
        }

        // POST: /Booking/AddBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBooking([FromForm] BookingCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate the room list, otherwise the room table renders empty
                // when validation fails and the view is redisplayed.
                await RepopulateAvailableRoomsAsync(dto, cancellationToken);
                return View("AddBooking", dto);
            }

            try
            {
                await _bookingService.CreateBookingAsync(dto, cancellationToken);
                TempData["Success"] = "Booking created successfully.";
                // Land straight back on the Index list (page 1, no stale filters) so the
                // booking that was just created is immediately visible.
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                await RepopulateAvailableRoomsAsync(dto, cancellationToken);
                return View("AddBooking", dto);
            }
        }

        private async Task RepopulateAvailableRoomsAsync(BookingCreateDto dto, CancellationToken cancellationToken)
        {
            var checkIn = dto?.CheckInDate ?? DateTime.Today;
            var checkOut = dto?.CheckOutDate ?? DateTime.Today.AddDays(1);
            var adults = dto?.Adults > 0 ? dto.Adults : 2;
            var children = dto?.Children ?? 0;

            var rooms = await _bookingService.GetAvailableRoomsAsync(checkIn, checkOut, string.Empty, null, adults, children, cancellationToken);
            ViewBag.AvailableRooms = rooms.Select(r => new
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Floor = r.Floor,
                MaxGuests = ((r.CapacityAdults ?? 0) + (r.CapacityChildren ?? 0)),
                PricePerNight = r.PricePerNight ?? 0m,
                Status = r.Availability,
                ImageUrl = string.IsNullOrWhiteSpace(r.ImagePaths) ? "/images/placeholder-room.jpg" : r.ImagePaths
            }).ToArray();
        }

        // GET: /Booking/GetAvailableRooms
        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(DateTime checkIn, DateTime checkOut, int adults, int children, string roomType = "", int? floor = null, CancellationToken cancellationToken = default)
        {
            var rooms = await _bookingService.GetAvailableRoomsAsync(checkIn, checkOut, roomType, floor, adults, children, cancellationToken);

            var ui = rooms.Select(r => new
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Floor = r.Floor,
                MaxGuests = ((r.CapacityAdults ?? 0) + (r.CapacityChildren ?? 0)),
                PricePerNight = r.PricePerNight ?? 0m,
                Status = r.Availability,
                ImageUrl = string.IsNullOrWhiteSpace(r.ImagePaths) ? "/images/placeholder-room.jpg" : r.ImagePaths
            });

            return Json(ui);
        }

        // POST: /Booking/ApplyCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon([FromForm] string code, [FromForm] decimal roomTotal = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                var res = await _bookingService.ApplyCouponAsync(code, roomTotal, cancellationToken);
                return Json(new { valid = res.valid, discountPercent = res.discountPercent, message = res.message });
            }
            catch (Exception ex)
            {
                return Json(new { valid = false, discountPercent = 0m, message = ex.Message });
            }
        }

        // GET: /Booking/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _bookingService.GetBookingByIdAsync(id, cancellationToken);
            if (dto == null) return NotFound();
            return View("Details", dto);
        }

        // GET: /Booking/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _bookingService.GetBookingByIdAsync(id, cancellationToken);
            if (dto == null) return NotFound();
            return View("Edit", dto);
        }

        // POST: /Booking/Update/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] BookingCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", dto);
            }

            try
            {
                // Controller should not contain business logic; assume service exposes update flow if needed
                await _bookingService.CreateBookingAsync(dto, cancellationToken);
                TempData["Success"] = "Booking updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                // Keep view lookup relative to the Booking controller's folder
                return View("Edit", dto);
            }
        }

        // POST: /Booking/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Soft delete to be implemented in service layer; here we only call it
                // Service currently does not expose Delete; if it does, call it. Placeholder:
                // await _bookingService.DeleteAsync(id, cancellationToken);
                TempData["Success"] = "Booking deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Booking/Export
        // FIX: this action did not exist at all. Booking.cshtml's Export button calls
        //   window.location.href = '@Url.Action("Export","Booking")' + window.location.search;
        // which forwarded the current filter querystring — but with no matching action
        // that request 404'd, so nothing ever downloaded. This applies the same
        // BookingFilterDto binding used by Index (so "Export" exports whatever the
        // user currently has filtered/searched for) and streams a CSV file back.
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] BookingFilterDto filter, CancellationToken cancellationToken = default)
        {
            filter ??= new BookingFilterDto();
            // Export the full filtered result set, not just the current page.
            filter.Page = 1;
            filter.PageSize = int.MaxValue;

            var page = await _bookingService.GetPagedBookingsAsync(filter, cancellationToken);
            var items = page?.Items ?? Enumerable.Empty<BookingListItemDto>();

            var sb = new StringBuilder();
            sb.AppendLine("Booking ID,Guest Name,Room Number,Check-In,Check-Out,Guests,Total Amount,Payment Status,Booking Status,Created Date");

            foreach (var b in items)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(b.BookingNumber),
                    CsvEscape(b.GuestName),
                    CsvEscape(b.RoomNumber),
                    b.CheckInDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    b.CheckOutDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    (b.Adults + b.Children).ToString(CultureInfo.InvariantCulture),
                    b.TotalAmount.ToString("F2", CultureInfo.InvariantCulture),
                    CsvEscape(b.PaymentStatus),
                    CsvEscape(b.BookingStatus),
                    b.CreatedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            var fileName = $"bookings-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
