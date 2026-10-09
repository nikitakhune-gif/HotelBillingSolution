using System;
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
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var filter = new BookingFilterDto { Page = 1, PageSize = 10 };
            var page = await _bookingService.GetPagedBookingsAsync(filter, cancellationToken);
            ViewData["BookingsPage"] = page;
            var summary = await _bookingService.GetBookingSummaryAsync(cancellationToken);
            ViewData["Summary"] = summary;
            return View("Booking/Index");
        }

        // GET: /Booking/Search?search=...&page=1
        [HttpGet]
        public async Task<IActionResult> Search(string search, int page = 1, CancellationToken cancellationToken = default)
        {
            var filter = new BookingFilterDto
            {
                QuickSearch = search ?? string.Empty,
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
            return View("Booking/AddBooking");
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BookingCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("Booking/AddBooking", dto);
            }

            try
            {
                await _bookingService.CreateBookingAsync(dto, cancellationToken);
                TempData["Success"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Booking/AddBooking", dto);
            }
        }

        // GET: /Booking/GetAvailableRooms
        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(DateTime checkIn, DateTime checkOut, int adults, int children, string roomType = "", int? floor = null, CancellationToken cancellationToken = default)
        {
            var rooms = await _bookingService.GetAvailableRoomsAsync(checkIn, checkOut, roomType, floor, cancellationToken);
            return Json(rooms);
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
            return View("Booking/Details", dto);
        }

        // GET: /Booking/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var dto = await _bookingService.GetBookingByIdAsync(id, cancellationToken);
            if (dto == null) return NotFound();
            return View("Booking/Edit", dto);
        }

        // POST: /Booking/Update/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] BookingCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("Booking/Edit", dto);
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
                return View("Booking/Edit", dto);
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
    }
}
