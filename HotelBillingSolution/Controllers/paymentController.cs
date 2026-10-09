using AutoMapper;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Enums;
using HotelBilling.Domain.Interfaces;
using HotelBillingWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBillingWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IRepository<Bill> _billRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IMapper _mapper;

        public PaymentController(IPaymentService paymentService, IRepository<Bill> billRepo, IRepository<Customer> customerRepo, IMapper mapper)
        {
            _paymentService = paymentService;
            _billRepo = billRepo;
            _customerRepo = customerRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string search, string status, DateTime? from, DateTime? to, int page = 1)
        {
            var model = new PaymentViewModel();

            var payments = (await _paymentService.GetAllAsync()).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                payments = payments.Where(p => (p.PaymentId ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                                              || (p.CustomerName ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(status))
                payments = payments.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (from.HasValue)
                payments = payments.Where(p => p.PaymentDate >= from.Value);

            if (to.HasValue)
                payments = payments.Where(p => p.PaymentDate <= to.Value);

            model.Page = page;
            model.PageSize = 10;
            model.Payments = payments.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();

            model.Summary = await _paymentService.GetSummaryAsync();

            var invoices = await _billRepo.GetAllAsync();
            model.InvoiceList = invoices.Select(i => new SelectListItem(i.Id.ToString(), i.Id.ToString()));
            // map full DTOs for view consumption
            model.Invoices = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Bill.BillDto>>(invoices);

            var customers = await _customerRepo.GetAllAsync();
            model.GuestList = customers.Select(g => new SelectListItem((g.FirstName + " " + g.LastName).Trim(), g.Id.ToString()));
            model.Guests = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Customer.CustomerDto>>(customers);

            model.PaymentMethodList = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(pm => new SelectListItem(pm.ToString(), ((int)pm).ToString()));

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new PaymentViewModel();

            var invoices = await _billRepo.GetAllAsync();
            model.InvoiceList = invoices.Select(i => new SelectListItem(i.Id.ToString(), i.Id.ToString()));
            model.Invoices = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Bill.BillDto>>(invoices);

            var customers2 = await _customerRepo.GetAllAsync();
            model.GuestList = customers2.Select(g => new SelectListItem((g.FirstName + " " + g.LastName).Trim(), g.Id.ToString()));
            model.Guests = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Customer.CustomerDto>>(customers2);

            model.PaymentMethodList = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(pm => new SelectListItem(pm.ToString(), ((int)pm).ToString()));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var invoices = await _billRepo.GetAllAsync();
                model.InvoiceList = invoices.Select(i => new SelectListItem(i.Id.ToString(), i.Id.ToString()));
                model.Invoices = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Bill.BillDto>>(invoices);

            var customers2 = await _customerRepo.GetAllAsync();
            model.GuestList = customers2.Select(g => new SelectListItem((g.FirstName + " " + g.LastName).Trim(), g.Id.ToString()));
            model.Guests = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Customer.CustomerDto>>(customers2);

                model.PaymentMethodList = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                    .Select(pm => new SelectListItem(pm.ToString(), ((int)pm).ToString()));

                return View(model);
            }

            await _paymentService.CreateAsync(model.Payment);

            TempData["Success"] = "Payment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();

            var model = new PaymentViewModel();

            // Map PaymentDto -> PaymentCreateDto manually (use existing DTO fields)
            model.Payment = new HotelBilling.Application.DTOs.PaymentCreateDto
            {
                PaymentId = payment.PaymentId,
                BillId = payment.BillId,
                CustomerId = payment.CustomerId,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method,
                TransactionId = payment.TransactionId,
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                CardLast4 = payment.CardLast4,
                CardBrand = payment.CardBrand,
                CardholderName = payment.CardholderName,
                SaveCard = payment.SaveCard,
                GatewayCardToken = payment.GatewayCardToken
            };

            var invoices = await _billRepo.GetAllAsync();
            model.InvoiceList = invoices.Select(i => new SelectListItem(i.Id.ToString(), i.Id.ToString()));
            model.Invoices = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Bill.BillDto>>(invoices);

                var customers = await _customerRepo.GetAllAsync();
                model.GuestList = customers.Select(g => new SelectListItem((g.FirstName + " " + g.LastName).Trim(), g.Id.ToString()));
                model.Guests = _mapper.Map<System.Collections.Generic.IEnumerable<HotelBilling.Application.DTOs.Customer.CustomerDto>>(customers);

            model.PaymentMethodList = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(pm => new SelectListItem(pm.ToString(), ((int)pm).ToString()));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var updateDto = new HotelBilling.Application.DTOs.PaymentUpdateDto
            {
                Id = id,
                Method = model.Payment.Method,
                TransactionId = model.Payment.TransactionId,
                ReferenceNumber = model.Payment.ReferenceNumber,
                Notes = model.Payment.Notes
                // Status can be set by admin UI if present
            };

            await _paymentService.UpdateAsync(updateDto);

            TempData["Success"] = "Payment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var success = await _paymentService.DeleteAsync(id);
            if (!success) return NotFound();

            TempData["Success"] = "Payment deleted.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        public async Task<IActionResult> Summary()
        {
            var summary = await _paymentService.GetSummaryAsync();
            return View(summary);
        }
    }
}
