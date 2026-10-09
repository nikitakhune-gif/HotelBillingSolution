using System.Diagnostics;
using HotelBillingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HotelBillingSolution.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HotelBilling.Application.Interfaces.IDashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger, HotelBilling.Application.Interfaces.IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var dto = await _dashboardService.GetDashboardAsync();

            // Map Application DTO to Web ViewModel
            var model = new HotelBillingSolution.Models.DashboardViewModel
            {
                TotalRooms = dto.TotalRooms,
                AvailableRooms = dto.AvailableRooms,
                TotalCustomers = dto.TotalCustomers,
                TodaysRevenue = dto.TodaysRevenue,
                TotalBookings = dto.TotalBookings,
                RevenueSeries = dto.RevenueSeries,
                RevenueLabels = dto.RevenueLabels,
                RoomStatusCounts = dto.RoomStatusCounts,
                OccupancyPercent = dto.OccupancyPercent,
                CheckInsToday = dto.CheckInsToday,
                CheckOutsToday = dto.CheckOutsToday,
                TodaysBookings = dto.TodaysBookings,
                RecentBookings = dto.RecentBookings.Select(r => new HotelBillingSolution.Models.RecentBookingVm
                {
                    Id = r.Id,
                    GuestName = r.GuestName,
                    BookingNumber = r.BookingNumber,
                    RoomLabel = r.RoomLabel,
                    CheckInDate = r.CheckInDate,
                    BookingStatus = r.BookingStatus,
                    AvatarUrl = r.AvatarUrl
                }).ToList(),
                TopCustomers = dto.TopCustomers.Select(c => new HotelBillingSolution.Models.TopCustomerVm
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Email = c.Email,
                    TotalSpent = c.TotalSpent,
                    StayCount = c.StayCount,
                    AvatarUrl = c.AvatarUrl
                }).ToList()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
