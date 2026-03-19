using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBilling.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        // Dependency Injection
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // 1. GET: Customer List
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        // 2. GET: Create Page
        public IActionResult Create()
        {
            return View();
        }

        // 3. POST: Create Customer
        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            await _customerService.CreateCustomerAsync(request);
            return RedirectToAction("Index");
        }

        // 4. GET: Edit Page
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            return View(customer);
        }

        // 5. POST: Update Customer
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            await _customerService.UpdateCustomerAsync(id, request);
            return RedirectToAction("Index");
        }

        // 6. Delete Customer
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction("Index");
        }
    }
}