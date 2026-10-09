using HotelBilling.Application.DTOs.Customer;
using HotelBilling.Application.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelBilling.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IWebHostEnvironment _env;

        // Dependency Injection
        public CustomerController(ICustomerService customerService, IWebHostEnvironment env)
        {
            _customerService = customerService;
            _env = env;
        }

        // 1. GET: Customer List
        public async Task<IActionResult> Index()
        {
            var customers = (await _customerService.GetAllCustomersAsync()).ToList();

            var summary = new CustomerSummaryDto
            {
                TotalCustomers = customers.Count,
                ActiveCustomers = customers.Count(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                NewCustomers = customers.Count(c => c.CreatedDate != default && c.CreatedDate.ToLocalTime().Year == DateTime.Now.Year && c.CreatedDate.ToLocalTime().Month == DateTime.Now.Month),
                VipCustomers = customers.Count(c => string.Equals(c.Membership, "VIP", StringComparison.OrdinalIgnoreCase) || string.Equals(c.CustomerType, "VIP", StringComparison.OrdinalIgnoreCase))
            };

            var vm = new CustomerIndexViewModel
            {
                Customers = customers,
                Summary = summary
            };

            return View(vm);
        }

        // 2. GET: Create Page
        public IActionResult AddCustomer()
        {
            // pass an empty DTO so the view's asp-for helpers have a model instance
            return View(new CustomerDto());
        }

        // 3. POST: Create Customer
        [HttpPost]
        public async Task<IActionResult> Create(CustomerDto request, IFormFile? profilePhoto)
        {
            if (!ModelState.IsValid)
                return View("AddCustomer", request);

            // Handle profile photo upload (optional)
            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                request.ProfilePhotoPath = await SaveProfilePhotoAsync(profilePhoto);
            }

            await _customerService.CreateCustomerAsync(request);
            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToAction("Index");
        }

        // GET: Export customers as CSV (accepts optional filter query params)
        [HttpGet]
        public async Task<IActionResult> ExportCustomers(string? search, string? type, string? status, string? city, string? country)
        {
            var customers = (await _customerService.GetAllCustomersAsync()).ToList();

            // apply simple server-side filters matching the client-side behavior
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                customers = customers.Where(c => (c.FirstName + " " + c.LastName + " " + c.Email + " " + c.MobileNumber).ToLower().Contains(term)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(type)) customers = customers.Where(c => string.Equals(c.CustomerType, type, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(status)) customers = customers.Where(c => string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(city)) customers = customers.Where(c => string.Equals(c.City, city, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(country)) customers = customers.Where(c => string.Equals(c.Country, country, StringComparison.OrdinalIgnoreCase)).ToList();

            // build CSV
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CustomerId,FirstName,LastName,Email,Mobile,Gender,City,Country,CustomerType,Status,RegistrationDate");
            foreach (var c in customers)
            {
                var reg = c.CreatedDate == default ? "" : c.CreatedDate.ToLocalTime().ToString("yyyy-MM-dd");
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    $"CUST-{c.Id:D4}", EscapeCsv(c.FirstName), EscapeCsv(c.LastName), EscapeCsv(c.Email), EscapeCsv(c.MobileNumber), EscapeCsv(c.Gender), EscapeCsv(c.City), EscapeCsv(c.Country), EscapeCsv(c.CustomerType), EscapeCsv(c.Status), reg);
                sb.AppendLine(line);
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"customers_export_{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            var v = value.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{v}\"" : v;
        }

        // Simple CSV line parser that supports quoted fields and escaped quotes ("")
        private static string[] ParseCsvLine(string? line)
        {
            if (string.IsNullOrEmpty(line)) return System.Array.Empty<string>();

            var values = new System.Collections.Generic.List<string>();
            var sb = new System.Text.StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++; // skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    values.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(ch);
                }
            }

            values.Add(sb.ToString());
            return values.ToArray();
        }

        // POST: Import customers from CSV
        [HttpPost]
        public async Task<IActionResult> ImportCustomers(IFormFile? csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["ImportSummary"] = "No file selected.";
                return RedirectToAction("Index");
            }

            var imported = 0;
            var skipped = 0;
            var errors = new List<string>();

            // Load existing emails once instead of re-querying per row
            var existingEmails = new HashSet<string>(
                (await _customerService.GetAllCustomersAsync()).Select(x => x.Email ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            using (var stream = csvFile.OpenReadStream())
            using (var reader = new System.IO.StreamReader(stream))
            {
                var header = await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCsvLine(line);
                    if (cols.Length < 5)
                    {
                        skipped++; errors.Add($"Invalid columns: {line}");
                        continue;
                    }
                    var first = cols.ElementAtOrDefault(1)?.Trim();
                    var last = cols.ElementAtOrDefault(2)?.Trim();
                    var email = cols.ElementAtOrDefault(3)?.Trim();
                    var mobile = cols.ElementAtOrDefault(4)?.Trim();

                    if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last) || string.IsNullOrWhiteSpace(email))
                    {
                        skipped++; errors.Add($"Missing required fields: {line}");
                        continue;
                    }

                    if (existingEmails.Contains(email!))
                    {
                        skipped++; continue;
                    }

                    var dto = new CustomerDto
                    {
                        FirstName = first ?? string.Empty,
                        LastName = last ?? string.Empty,
                        Email = email ?? string.Empty,
                        MobileNumber = mobile ?? string.Empty,
                        Gender = cols.ElementAtOrDefault(5)?.Trim(),
                        City = cols.ElementAtOrDefault(6)?.Trim(),
                        Country = cols.ElementAtOrDefault(7)?.Trim(),
                        CustomerType = cols.ElementAtOrDefault(8)?.Trim() ?? "Regular",
                        Status = cols.ElementAtOrDefault(9)?.Trim() ?? "Active"
                    };

                    await _customerService.CreateCustomerAsync(dto);
                    existingEmails.Add(dto.Email);
                    imported++;
                }
            }

            TempData["ImportSummary"] = $"Imported: {imported}, Skipped: {skipped}." + (errors.Any() ? " See logs." : "");
            return RedirectToAction("Index");
        }

        // 4. GET: Edit Page
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // 5. POST: Update Customer
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CustomerDto request, IFormFile? profilePhoto)
        {
            if (!ModelState.IsValid)
                return View(request);

            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                request.ProfilePhotoPath = await SaveProfilePhotoAsync(profilePhoto);
            }

            await _customerService.UpdateCustomerAsync(id, request);
            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToAction("Index");
        }

        // 6. Delete Customer
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            TempData["SuccessMessage"] = "Customer deleted successfully.";
            return RedirectToAction("Index");
        }

        // 7. GET: Customer Details (read-only profile)
        // NOTE: previously this incorrectly called DeleteCustomerAsync — fixed to fetch instead.
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // 8. GET: Booking history for a customer
        // NOTE: previously "BookingHistroy" (typo) also incorrectly called DeleteCustomerAsync — fixed and renamed.
        // TODO: replace the placeholder ViewBag.Bookings below with a real call once IBookingService
        // exposes a GetBookingsByCustomerIdAsync(id) (or similar) method.
        public async Task<IActionResult> BookingHistory(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index");
            }
            // Fetch real bookings for this customer using BookingController/BookingService
            // The BookingService exposes GetPagedBookingsAsync that we can reuse by
            // creating a filter that restricts GuestName or CustomerId. There is no
            // IBookingService in this controller, so use Url.Action to redirect the
            // Create Booking button to the Booking/AddBooking action with customerId.
            // However, to keep controller-layer changes minimal we will call the
            // BookingService via dependency injection if available through HttpContext
            // RequestServices. This avoids changing constructor signature here.
            try
            {
                var bookingService = HttpContext.RequestServices.GetService(typeof(HotelBilling.Application.Interfaces.IBookingService)) as HotelBilling.Application.Interfaces.IBookingService;
                if (bookingService != null)
                {
                    var filter = new HotelBilling.Application.DTOs.Booking.BookingFilterDto { GuestName = customer.FullName, Page = 1, PageSize = 50 };
                    var page = await bookingService.GetPagedBookingsAsync(filter);
                    ViewBag.Bookings = (page?.Items ?? Enumerable.Empty<HotelBilling.Application.DTOs.Booking.BookingListItemDto>()).ToList();
                }
                else
                {
                    ViewBag.Bookings = new List<HotelBilling.Application.DTOs.Booking.BookingListItemDto>();
                }
            }
            catch
            {
                ViewBag.Bookings = new List<HotelBilling.Application.DTOs.Booking.BookingListItemDto>();
            }
            return View(customer);
        }

        // 9. GET: Invoices for a customer
        // NOTE: previously "invoice" (lowercase) also incorrectly called DeleteCustomerAsync — fixed and renamed.
        // TODO: replace the placeholder ViewBag.Invoices below with a real call once IInvoiceService
        // exposes a GetInvoicesByCustomerIdAsync(id) (or similar) method.
        public async Task<IActionResult> Invoice(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Index");
            }
            try
            {
                var billingService = HttpContext.RequestServices.GetService(typeof(HotelBilling.Application.Interfaces.IBillingService)) as HotelBilling.Application.Interfaces.IBillingService;
                if (billingService != null)
                {
                    var bills = (await billingService.GetAllAsync()).Where(b => b.CustomerId == id).ToList();
                    ViewBag.Invoices = bills;
                }
                else
                {
                    ViewBag.Invoices = new List<HotelBilling.Application.DTOs.Bill.BillDto>();
                }
            }
            catch
            {
                ViewBag.Invoices = new List<HotelBilling.Application.DTOs.Bill.BillDto>();
            }
            return View(customer);
        }

        // Shared helper: saves an uploaded profile photo under wwwroot/uploads/customers
        // and returns the relative path to store on the DTO.
        private async Task<string> SaveProfilePhotoAsync(IFormFile profilePhoto)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "customers");
            Directory.CreateDirectory(uploads);
            var fileName = $"cust_{Guid.NewGuid()}{Path.GetExtension(profilePhoto.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await profilePhoto.CopyToAsync(stream);
            }
            return $"/uploads/customers/{fileName}";
        }
    }
}