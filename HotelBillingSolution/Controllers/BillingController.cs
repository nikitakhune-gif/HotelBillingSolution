using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBillingWeb.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly ICustomerService _customerService;

        // Primary constructor
        public BillingController(IBillingService billingService, ICustomerService customerService)
            => (_billingService, _customerService) = (billingService, customerService);

        // GET: /Billing
        public async Task<IActionResult> Index()
        {
            var bills = await _billingService.GetAllAsync();
            return View(bills);
        }

        // GET: /Billing/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View();
        }

        // POST: /Billing/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateBillRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _customerService.GetAllAsync();
                return View(request);
            }

            try
            {
                var billResponse = await _billingService.GenerateBillAsync(request);
                if (billResponse != null && billResponse.Id > 0)
                {
                    // Redirect to details of the created bill so user can see generated Id
                    return RedirectToAction(nameof(Details), new { id = billResponse.Id });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Customers = await _customerService.GetAllAsync();
                return View(request);
            }
        }

        // GET: /Billing/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var bill = await _billingService.GetByIdAsync(id);
            if (bill == null) return NotFound();
            return View(bill);
        }
    }
}