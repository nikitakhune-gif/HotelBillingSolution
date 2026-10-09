
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using HotelBilling.Application.Interfaces;
using HotelBilling.Application.DTOs.Settings;

namespace HotelBillingWeb.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService
                ?? throw new ArgumentNullException(nameof(settingsService));
        }


        // =========================================================
        // SETTINGS
        // =========================================================

        // GET: /Settings
        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = await _settingsService
                    .GetSettingsAsync(cancellationToken);

                if (settings == null)
                {
                    TempData["Error"] = "Settings not found.";
                    return View("Index");
                }

                return View("Index", settings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Index");
            }
        }


        // POST: /Settings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            [FromForm] SettingsDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", dto);
            }

            try
            {
                await _settingsService.UpdateSettingsAsync(
                    dto,
                    cancellationToken);

                TempData["Success"] =
                    "Settings updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Index", dto);
            }
        }


        // =========================================================
        // SUPPORT DESK
        // =========================================================

        // GET: /Settings/SupportDesk
        [HttpGet]
        public async Task<IActionResult> SupportDesk(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var tickets = await _settingsService
                    .GetSupportTicketsAsync(cancellationToken);

                return View("SupportDesk", tickets);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("SupportDesk");
            }
        }


        // GET: /Settings/SupportDesk/Details/5
        [HttpGet]
        public async Task<IActionResult> SupportDeskDetails(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var ticket = await _settingsService
                    .GetSupportTicketByIdAsync(
                        id,
                        cancellationToken);

                if (ticket == null)
                {
                    return NotFound();
                }

                return View("SupportDeskDetails", ticket);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SupportDesk));
            }
        }


        // POST: /Settings/CreateSupportTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSupportTicket(
            [FromForm] CreateSupportTicketDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("SupportDesk", dto);
            }

            try
            {
                await _settingsService.CreateSupportTicketAsync(
                    dto,
                    cancellationToken);

                TempData["Success"] =
                    "Support ticket created successfully.";

                return RedirectToAction(nameof(SupportDesk));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("SupportDesk", dto);
            }
        }


        // POST: /Settings/UpdateSupportTicket/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSupportTicket(
            int id,
            [FromForm] UpdateSupportTicketDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("SupportDeskDetails", dto);
            }

            try
            {
                await _settingsService.UpdateSupportTicketAsync(
                    id,
                    dto,
                    cancellationToken);

                TempData["Success"] =
                    "Support ticket updated successfully.";

                return RedirectToAction(nameof(SupportDesk));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("SupportDeskDetails", dto);
            }
        }


        // =========================================================
        // ALERTS
        // =========================================================

        // GET: /Settings/Alerts
        [HttpGet]
        public async Task<IActionResult> Alerts(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var alerts = await _settingsService
                    .GetAlertsAsync(cancellationToken);

                return View("Alerts", alerts);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Alerts");
            }
        }


        // GET: /Settings/Alerts/Details/5
        [HttpGet]
        public async Task<IActionResult> AlertDetails(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var alert = await _settingsService
                    .GetAlertByIdAsync(
                        id,
                        cancellationToken);

                if (alert == null)
                {
                    return NotFound();
                }

                return View("AlertDetails", alert);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Alerts));
            }
        }


        // POST: /Settings/Alerts/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAlertAsRead(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                await _settingsService.MarkAlertAsReadAsync(
                    id,
                    cancellationToken);

                TempData["Success"] =
                    "Alert marked as read.";

                return RedirectToAction(nameof(Alerts));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Alerts));
            }
        }


        // POST: /Settings/Alerts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAlert(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                await _settingsService.DeleteAlertAsync(
                    id,
                    cancellationToken);

                TempData["Success"] =
                    "Alert deleted successfully.";

                return RedirectToAction(nameof(Alerts));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Alerts));
            }
        }
    }
}
