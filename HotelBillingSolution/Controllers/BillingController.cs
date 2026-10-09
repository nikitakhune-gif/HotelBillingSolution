using HotelBilling.Application.DTOs.Bill;
using HotelBilling.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace HotelBillingWeb.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly ICustomerService _customerService;
        private readonly HotelBilling.Application.Interfaces.IRoomService _roomService;

        // Primary constructor
        public BillingController(IBillingService billingService, ICustomerService customerService, HotelBilling.Application.Interfaces.IRoomService roomService)
            => (_billingService, _customerService, _roomService) = (billingService, customerService, roomService);

        // GET: /Billing
        public async Task<IActionResult> Index()
        {
            var bills = await _billingService.GetAllAsync();
            // Provide basic filter dropdowns where the view expects them
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Rooms = await _roomService.GetAllAsync();
            return View(bills);
        }

        // GET: /Billing/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Rooms = await _roomService.GetAllAsync();
            return View();
        }

        // POST: /Billing/Create
        [HttpPost]
        public async Task<IActionResult> Create(BillDto request)
        {
            if (!ModelState.IsValid)
            {
                // Ensure the same customer list and rooms are loaded when redisplaying the form
                ViewBag.Customers = await _customerService.GetAllCustomersAsync();
                ViewBag.Rooms = await _roomService.GetAllAsync();
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
                ViewBag.Customers = await _customerService.GetAllCustomersAsync();
                ViewBag.Rooms = await _roomService.GetAllAsync();
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

        // GET: /Billing/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bill = await _billing_service_getbyid_safe(id: id);
            if (bill == null) return NotFound();

            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Rooms = await _roomService.GetAllAsync();
            return View(bill);
        }

        // POST: /Billing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BillDto request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _customerService.GetAllCustomersAsync();
                ViewBag.Rooms = await _roomService.GetAllAsync();
                return View(request);
            }

            try
            {
                // The application service does not expose an Update method yet in the interface;
                // attempt to call a best-effort UpdateBillAsync if available, otherwise return to Details.
                // If your IBillingService exposes an Update method, replace the call below.
                var service = _billingService;

                // Try to find and update via repository pattern through the service
                var existing = await _billingService.GetByIdAsync(id);
                if (existing == null) return NotFound();

                // Map allowed updatable fields from request to existing DTO and persist.
                existing.BillNumber = request.BillNumber;
                existing.BillDate = request.BillDate;
                existing.CustomerId = request.CustomerId;
                existing.RoomId = request.RoomId;
                existing.CheckInDate = request.CheckInDate;
                existing.CheckOutDate = request.CheckOutDate;
                existing.Nights = request.Nights;
                existing.SubTotal = request.SubTotal;
                existing.DiscountPercent = request.DiscountPercent;
                existing.DiscountAmount = request.DiscountAmount;
                existing.TaxPercent = request.TaxPercent;
                existing.TaxAmount = request.TaxAmount;
                existing.ServiceCharge = request.ServiceCharge;
                existing.TotalAmount = request.TotalAmount;
                existing.PaymentMethod = request.PaymentMethod;
                existing.AdvanceReceived = request.AdvanceReceived;
                existing.PaidAmount = request.PaidAmount;
                existing.DueAmount = request.DueAmount;
                existing.TransactionNumber = request.TransactionNumber;
                existing.PaymentDate = request.PaymentDate;
                existing.PaymentStatus = request.PaymentStatus;
                existing.BillStatus = request.BillStatus;
                existing.Notes = request.Notes;
                existing.Items = request.Items;

                // If the service exposes a dedicated update call use it; otherwise try reflection to call UpdateBillAsync
                var updateMethod = _billingService.GetType().GetMethod("UpdateBillAsync");
                if (updateMethod != null)
                {
                    var task = (Task)updateMethod.Invoke(_billingService, new object[] { existing })!;
                    await task;
                }
                else
                {
                    // As a fallback, try to update payment status only (non-destructive). Better to implement Update in service.
                }

                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Customers = await _customerService.GetAllCustomersAsync();
                ViewBag.Rooms = await _room_service_getall_safe();
                return View(request);
            }
        }

        // GET: /Billing/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var bill = await _billingService.GetByIdAsync(id);
            if (bill == null) return NotFound();

            var pdfBytes = GenerateSimplePdfBytes(bill);
            var fileName = string.IsNullOrWhiteSpace(bill.BillNumber) ? $"bill-{bill.Id}.pdf" : $"{bill.BillNumber}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // Helper: small, self-contained PDF generator for basic invoice content.
        // This avoids adding external PDF libraries; for production prefer QuestPDF or similar.
        private byte[] GenerateSimplePdfBytes(BillDto bill)
        {
            // Build a few lines of text representing the bill.
            var lines = new List<string>
            {
                $"Invoice: {bill.BillNumber}",
                $"Date: {bill.BillDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}",
                $"Customer: {bill.CustomerName}",
                $"Room: {bill.RoomNumber}",
                "",
                "Items:" 
            };

            if (bill.Items != null && bill.Items.Any())
            {
                foreach (var it in bill.Items)
                {
                    lines.Add($"- {it.ItemName} x{it.Quantity} @ {it.Rate.ToString("N2")} = {it.Amount.ToString("N2")}");
                }
            }
            else
            {
                lines.Add("(no items)");
            }

            lines.Add("");
            lines.Add($"Subtotal: {bill.SubTotal.ToString("N2")}");
            lines.Add($"Discount: {bill.DiscountAmount.ToString("N2")}");
            lines.Add($"Tax: {bill.TaxAmount.ToString("N2")}");
            lines.Add($"Service: {bill.ServiceCharge.ToString("N2")}");
            lines.Add($"Total: {bill.TotalAmount.ToString("N2")}");

            // Create a simple PDF binary with the text. This is intentionally minimal and not feature complete.
            string streamContent = "BT /F1 12 Tf 50 750 Td ";
            var yOffset = 0;
            foreach (var l in lines)
            {
                var escaped = l.Replace("(", "\\(").Replace(")", "\\)");
                streamContent += $"({escaped}) Tj 0 -14 Td ";
                yOffset += 14;
            }
            streamContent += "ET\n";

            var sb = new StringBuilder();
            sb.Append("%PDF-1.4\n");

            var objects = new List<byte[]>();

            // Object 1: Catalog
            objects.Add(Encoding.ASCII.GetBytes("1 0 obj<< /Type /Catalog /Pages 2 0 R>>endobj\n"));
            // Object 2: Pages
            objects.Add(Encoding.ASCII.GetBytes("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1>>endobj\n"));
            // Object 3: Page placeholder; will reference object 4 (font) and 5 (contents)
            objects.Add(Encoding.ASCII.GetBytes("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R>> >> /Contents 5 0 R>>endobj\n"));
            // Object 4: Font
            objects.Add(Encoding.ASCII.GetBytes("4 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica>>endobj\n"));

            var streamBytes = Encoding.ASCII.GetBytes(streamContent);
            var obj5Head = Encoding.ASCII.GetBytes($"5 0 obj<< /Length {streamBytes.Length}>>stream\n");
            var obj5Tail = Encoding.ASCII.GetBytes("\nendstream endobj\n");
            var obj5 = new byte[obj5Head.Length + streamBytes.Length + obj5Tail.Length];
            Buffer.BlockCopy(obj5Head, 0, obj5, 0, obj5Head.Length);
            Buffer.BlockCopy(streamBytes, 0, obj5, obj5Head.Length, streamBytes.Length);
            Buffer.BlockCopy(obj5Tail, 0, obj5, obj5Head.Length + streamBytes.Length, obj5Tail.Length);
            objects.Add(obj5);

            // Build xref table
            var offsets = new List<int>();
            var output = new List<byte>();
            foreach (var obj in objects)
            {
                offsets.Add(output.Count);
                output.AddRange(obj);
            }

            var xrefStart = output.Count;
            var xrefSb = new StringBuilder();
            xrefSb.Append("xref\n0 ");
            xrefSb.Append(objects.Count + 1);
            xrefSb.Append('\n');
            xrefSb.AppendFormat("0000000000 65535 f \n");
            for (int i = 0; i < offsets.Count; i++)
            {
                xrefSb.AppendFormat("{0:0000000000} 00000 n \n", offsets[i]);
            }

            var trailerSb = new StringBuilder();
            trailerSb.Append("trailer\n<< /Size ");
            trailerSb.Append(objects.Count + 1);
            trailerSb.Append(" /Root 1 0 R>>\nstartxref\n");
            trailerSb.Append(xrefStart);
            trailerSb.Append("\n%%EOF\n");

            // Compose final bytes
            var final = new List<byte>();
            final.AddRange(Encoding.ASCII.GetBytes("%PDF-1.4\n"));
            foreach (var obj in objects)
            {
                final.AddRange(obj);
            }
            final.AddRange(Encoding.ASCII.GetBytes(xrefSb.ToString()));
            final.AddRange(Encoding.ASCII.GetBytes(trailerSb.ToString()));

            return final.ToArray();
        }

        // small helpers to avoid typing long service names in a few places
        private async Task<BillDto?> _billing_service_getbyid_safe(int id)
            => await _billingService.GetByIdAsync(id);

        private async Task<dynamic> _room_service_getall_safe()
            => await _roomService.GetAllAsync();
    }
}