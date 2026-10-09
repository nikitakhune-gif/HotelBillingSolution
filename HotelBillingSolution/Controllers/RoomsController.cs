using Microsoft.AspNetCore.Mvc;
using HotelBilling.Application.Interfaces;
using HotelBilling.Application.DTOs.Room;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System;

namespace HotelBilling.Web.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _env;

        public RoomsController(IRoomService roomService, IWebHostEnvironment env)
        {
            _roomService = roomService;
            _env = env;
        }

        // GET: Rooms list view
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllForViewAsync();
            return View(rooms);
        }

        // GET: returns JSON list for AJAX
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _roomService.GetAllAsync();
            return Json(rooms);
        }

        // GET: Single room
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return Json(room);
        }

        // GET: Create page
        public IActionResult AddRoom()
        {
            return View();
        }

        // POST: Import CSV file
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ImportResult"] = "No file selected.";
                return RedirectToAction("Index");
            }

            var errors = new List<string>();
            var created = 0;

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string? header = await reader.ReadLineAsync();
                if (header == null)
                {
                    TempData["ImportResult"] = "File is empty.";
                    return RedirectToAction("Index");
                }

                var cols = header.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                int lineNo = 1;
                while (!reader.EndOfStream)
                {
                    lineNo++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',', StringSplitOptions.TrimEntries);

                    // Simple CSV mapping by header names (best-effort)
                    try
                    {
                        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < Math.Min(cols.Length, parts.Length); i++)
                        {
                            dict[cols[i]] = parts[i];
                        }

                        dict.TryGetValue("RoomNumber", out var roomNo);
                        dict.TryGetValue("RoomName", out var roomName);
                        dict.TryGetValue("RoomType", out var roomType);
                        dict.TryGetValue("Floor", out var floor);
                        dict.TryGetValue("PricePerNight", out var priceStr);
                        dict.TryGetValue("CapacityAdults", out var capAdults);
                        dict.TryGetValue("CapacityChildren", out var capChildren);
                        dict.TryGetValue("Availability", out var availability);
                        dict.TryGetValue("HousekeepingStatus", out var hk);

                        if (string.IsNullOrWhiteSpace(roomNo) || string.IsNullOrWhiteSpace(roomName))
                        {
                            errors.Add($"Line {lineNo}: RoomNumber and RoomName are required.");
                            continue;
                        }

                        // duplicate check
                        if (await _roomService.ExistsByRoomNumberAsync(roomNo!))
                        {
                            errors.Add($"Line {lineNo}: Room number {roomNo} already exists.");
                            continue;
                        }

                        var dto = new CreateRoomDto
                        {
                            RoomNumber = roomNo ?? string.Empty,
                            RoomName = roomName ?? string.Empty,
                            RoomType = roomType ?? string.Empty,
                            Floor = floor ?? string.Empty,
                            HousekeepingStatus = hk,
                            Availability = availability
                        };

                        if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) dto.PricePerNight = p;
                        if (int.TryParse(capAdults, out var ca)) dto.CapacityAdults = ca;
                        if (int.TryParse(capChildren, out var cc)) dto.CapacityChildren = cc;

                        await _roomService.CreateAsync(dto);
                        created++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {lineNo}: {ex.Message}");
                    }
                }
            }

            var msg = $"Imported {created} rooms.";
            if (errors.Any()) msg += " Errors: " + string.Join("; ", errors.Take(10));
            TempData["ImportResult"] = msg;
            return RedirectToAction("Index");
        }

        // GET: Export CSV
        public async Task<IActionResult> Export()
        {
            var rooms = await _roomService.GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("RoomNumber,RoomName,RoomType,Floor,PricePerNight,CapacityAdults,CapacityChildren,Availability,HousekeepingStatus,CreatedDate");
            foreach (var r in rooms)
            {
                var line = string.Format(
                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    EscapeCsv(r.RoomNumber), EscapeCsv(r.RoomName), EscapeCsv(r.RoomType), EscapeCsv(r.Floor),
                    (r.PricePerNight.HasValue ? r.PricePerNight.Value.ToString(CultureInfo.InvariantCulture) : ""),
                    (r.CapacityAdults?.ToString() ?? ""), (r.CapacityChildren?.ToString() ?? ""), EscapeCsv(r.Availability), EscapeCsv(r.HousekeepingStatus), r.CreatedDate.ToString("o")
                );
                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"rooms_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);

            static string EscapeCsv(string? s)
            {
                if (string.IsNullOrEmpty(s)) return string.Empty;
                if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
                {
                    return "\"" + s.Replace("\"", "\"\"") + "\"";
                }
                return s;
            }
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(CreateRoomDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            // duplicate check
            if (await _roomService.ExistsByRoomNumberAsync(request.RoomNumber))
            {
                ModelState.AddModelError(nameof(request.RoomNumber), "Room number already exists.");
                return View(request);
            }

            // handle uploaded images from form (input name="images")
            var files = Request.Form?.Files;
            if (files != null && files.Count > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "rooms");
                Directory.CreateDirectory(uploadsDir);
                var saved = new List<string>();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                foreach (var file in files)
                {
                    try
                    {
                        if (file == null || file.Length == 0) continue;
                        var ext = Path.GetExtension(file.FileName);
                        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext.ToLowerInvariant())) continue;

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var savePath = Path.Combine(uploadsDir, fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        saved.Add($"/uploads/rooms/{fileName}");
                    }
                    catch
                    {
                        // ignore single file errors — continue with others
                    }
                }

                if (saved.Any())
                {
                    // store as semicolon-separated paths
                    request.ImagePaths = string.Join(";", saved);
                }
            }

            await _roomService.CreateAsync(request);
            return RedirectToAction("Index");
        }

        // GET: Edit page
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateRoomDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            // load existing room to preserve/modify current image paths
            var existing = await _roomService.GetByIdAsync(id);
            var existingPaths = existing?.ImagePaths?.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();

            // handle removed existing images (field added by JS as comma-separated)
            var removed = Request.Form["RemovedImageUrls"].ToString();
            if (!string.IsNullOrWhiteSpace(removed))
            {
                var removeList = removed.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                foreach (var r in removeList)
                {
                    existingPaths.RemoveAll(x => string.Equals(x, r, StringComparison.OrdinalIgnoreCase));
                }
            }

            // handle newly uploaded images
            var files = Request.Form?.Files;
            if (files != null && files.Count > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "rooms");
                Directory.CreateDirectory(uploadsDir);
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                foreach (var file in files)
                {
                    try
                    {
                        if (file == null || file.Length == 0) continue;
                        var ext = Path.GetExtension(file.FileName);
                        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext.ToLowerInvariant())) continue;

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var savePath = Path.Combine(uploadsDir, fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        existingPaths.Add($"/uploads/rooms/{fileName}");
                    }
                    catch
                    {
                        // ignore single file errors
                    }
                }
            }

            request.ImagePaths = existingPaths.Any() ? string.Join(";", existingPaths) : null;

            await _roomService.UpdateAsync(id, request);
            return RedirectToAction("Index");
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            await _roomService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
