using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelBillingWeb.Controllers
{
    public class ReportsController : Controller
    {
        // GET: /Reports
        [HttpGet]
        public IActionResult Index(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var model = new ReportsViewModel
            {
                FromDate = fromDate ?? DateTime.Today.AddDays(-15),
                ToDate = toDate ?? DateTime.Today,

                TotalRevenue = 284920m,
                TotalBookings = 132,
                TotalPayments = 108,
                Occupancy = 78,
                HousekeepingTasks = 452
            };

            return View(model);
        }

        // GET: /Reports/Download
        [HttpGet]
        public IActionResult Download(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string reportType = "All")
        {
            // TODO:
            // Connect with ReportService and generate
            // Excel / PDF / CSV according to reportType.

            TempData["Success"] = "Report generated successfully.";

            return RedirectToAction(nameof(Index), new
            {
                fromDate,
                toDate
            });
        }

        // GET: /Reports/Details/Revenue
        [HttpGet]
        public IActionResult Details(string reportType)
        {
            if (string.IsNullOrWhiteSpace(reportType))
                return BadRequest();

            ViewBag.ReportType = reportType;

            return View("Details");
        }

        // Detailed report pages
        [HttpGet]
        public IActionResult Room()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Customer()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Revenue()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Booking()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Payment()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Housekeeping()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Billing()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Occupancy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult StaffPerformance()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Custom()
        {
            return View();
        }
    }


    public class ReportsViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPayments { get; set; }
        public int Occupancy { get; set; }
        public int HousekeepingTasks { get; set; }
    }
}