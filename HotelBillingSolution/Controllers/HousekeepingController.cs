
using HotelBilling.Infrastructure.Data.DbContext;
using HotelBillingSolution.Application.DTOs.Housekeeping;
using HotelBillingSolution.Domain.Entities;
using HotelBillingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBillingWeb.Controllers
{
    public class HousekeepingController : Controller
    {
        private readonly AppDbContext _context;

        public HousekeepingController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: parse nullable floor text (e.g. "1", "Ground") to int for dashboard display.
        // Returns 0 when parsing fails or value is null.
        private static int ParseFloorToInt(string? floorText)
        {
            if (string.IsNullOrWhiteSpace(floorText))
                return 0;

            if (int.TryParse(floorText, out var v))
                return v;

            return 0;
        }

        // =========================================================
        // HOUSEKEEPING DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? priority,
            int? assignedTo,
            string? taskType,
            int? floor,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1,
            int pageSize = 10)
        {
            var totalRooms = await _context.Rooms.CountAsync();

            var taskQuery = _context.HousekeepingTasks
                .Include(x => x.Room)
                .Include(x => x.AssignedToStaff)
                .Include(x => x.Booking)
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                taskQuery = taskQuery.Where(x =>
                    (x.Room != null && x.Room.RoomNumber.Contains(search)) ||
                    (x.GuestName != null && x.GuestName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                taskQuery = taskQuery.Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(priority))
                taskQuery = taskQuery.Where(x => x.Priority == priority);

            if (assignedTo.HasValue)
                taskQuery = taskQuery.Where(x => x.AssignedToStaffId == assignedTo.Value);

            if (!string.IsNullOrWhiteSpace(taskType))
                taskQuery = taskQuery.Where(x => x.TaskType == taskType);

            if (floor.HasValue)
                taskQuery = taskQuery.Where(x => x.Room != null && x.Room.Floor == floor.Value.ToString());

            if (dateFrom.HasValue)
                taskQuery = taskQuery.Where(x => x.TaskDate >= dateFrom.Value.Date);

            if (dateTo.HasValue)
            {
                var toDate = dateTo.Value.Date.AddDays(1);
                taskQuery = taskQuery.Where(x => x.TaskDate < toDate);
            }

            var totalTasks = await taskQuery.CountAsync();

            // -----------------------------
            // Summary counts (computed BEFORE they're used below)
            // -----------------------------

            var cleanRooms = await taskQuery.CountAsync(x => x.Status == "Clean");
            var inProgress = await taskQuery.CountAsync(x => x.Status == "InProgress");
            var dueForCleaning = await taskQuery.CountAsync(x => x.Status == "DueForCleaning");
            var outOfOrder = await taskQuery.CountAsync(x => x.Status == "OutOfOrder");

            // -----------------------------
            // Pagination
            // -----------------------------

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var tasks = await taskQuery
                .OrderByDescending(x => x.TaskDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // -----------------------------
            // Build dashboard DTO
            // -----------------------------

            var dashboard = new HousekeepingDashboardDto();

            dashboard.RoomStatusList = tasks.Select(t => new RoomStatusRowDto
            {
                HousekeepingTaskId = t.HousekeepingTaskId,
                RoomNo = t.Room != null ? t.Room.RoomNumber : string.Empty,
                RoomType = t.Room != null ? t.Room.RoomType : string.Empty,
                Floor = ParseFloorToInt(t.Floor ?? t.Room?.Floor),
                GuestName = t.GuestName,
                ReservationStatusText = t.Booking != null ? "Reserved" : null,
                Status = t.Status ?? "Clean",
                AssignedTo = t.AssignedToStaff != null ? t.AssignedToStaff.Username : null,
                LastCleaned = t.LastCleaned
            }).ToList();

            dashboard.Pagination.PageNumber = page;
            dashboard.Pagination.PageSize = pageSize;
            dashboard.Pagination.TotalRecords = totalTasks;

            dashboard.Filters.Search = search;
            dashboard.Filters.Status = status;
            dashboard.Filters.Priority = priority;
            dashboard.Filters.AssignedTo = assignedTo;
            dashboard.Filters.TaskType = taskType;
            dashboard.Filters.Floor = floor;
            dashboard.Filters.DateFrom = dateFrom;
            dashboard.Filters.DateTo = dateTo;

            dashboard.Summary.TotalRooms = totalRooms;
            dashboard.Summary.CleanRooms = cleanRooms;
            dashboard.Summary.InProgressRooms = inProgress;
            dashboard.Summary.DueForCleaningRooms = dueForCleaning;
            dashboard.Summary.OutOfOrderRooms = outOfOrder;

            if (totalRooms > 0)
            {
                dashboard.Summary.CleanPercent = Math.Round((decimal)cleanRooms / totalRooms * 100, 1);
                dashboard.Summary.InProgressPercent = Math.Round((decimal)inProgress / totalRooms * 100, 1);
                dashboard.Summary.DueForCleaningPercent = Math.Round((decimal)dueForCleaning / totalRooms * 100, 1);
                dashboard.Summary.OutOfOrderPercent = Math.Round((decimal)outOfOrder / totalRooms * 100, 1);
            }

            // Staff on duty — NOTE: assumes AppUser represents staff (see chat notes).
            dashboard.StaffOnDuty = await _context.AppUsers
                .Select(s => new StaffOnDutyDto
                {
                    StaffMemberId = s.Id,
                    Name = s.Username,
                    RoomsAssigned = _context.HousekeepingTasks
                        .Count(t => t.AssignedToStaffId == s.Id && t.Status != "Completed" && !t.IsDeleted)
                })
                .ToListAsync();

            var denom = totalTasks == 0 ? 1 : totalTasks;
            dashboard.CleanlinessScore.Score = (int)Math.Round((cleanRooms / (double)denom) * 100);
            dashboard.CleanlinessScore.Label =
                dashboard.CleanlinessScore.Score >= 90 ? "Excellent" :
                dashboard.CleanlinessScore.Score >= 70 ? "Good" : "Needs Attention";

            return View(dashboard);
        }

        // =========================================================
        // CREATE HOUSEKEEPING TASK - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            var dto = new HousekeepingTaskDto
            {
                TaskDate = DateTime.Today,
                DueDate = DateTime.Today,
                Status = "Pending"
            };

            return View(dto);
        }

        // =========================================================
        // CREATE HOUSEKEEPING TASK - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HousekeepingTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(dto);
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == dto.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Selected room does not exist.");
                await LoadDropdowns();
                return View(dto);
            }

            if (dto.AssignedTo.HasValue)
            {
                var staff = await _context.AppUsers
                    .FirstOrDefaultAsync(x => x.Id == dto.AssignedTo.Value);

                if (staff == null)
                {
                    ModelState.AddModelError("AssignedTo", "Selected staff is not available.");
                    await LoadDropdowns();
                    return View(dto);
                }
            }

            if (dto.DueDate < dto.TaskDate)
            {
                ModelState.AddModelError("DueDate", "Due date cannot be before task date.");
                await LoadDropdowns();
                return View(dto);
            }

            var task = new HousekeepingTask
            {
                RoomId = dto.RoomId,
                AssignedToStaffId = dto.AssignedTo,
                GuestName = dto.GuestName,

                TaskType = dto.TaskType,
                Priority = dto.Priority,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,

                TaskDate = dto.TaskDate,
                DueDate = dto.DueDate,
                Floor = dto.Floor.HasValue ? dto.Floor.Value.ToString() : room.Floor,

                Description = dto.Description,
                Instructions = dto.Instructions,
                Notes = dto.Notes,

                CreatedAt = DateTime.UtcNow
            };

            _context.HousekeepingTasks.Add(task);
            await _context.SaveChangesAsync();

            // TODO: handle dto.Attachments upload + HousekeepingTaskAttachment rows here.

            TempData["SuccessMessage"] = "Housekeeping task created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.HousekeepingTasks
                .Include(x => x.Room)
                .Include(x => x.AssignedToStaff)
                .Include(x => x.Booking)
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.HousekeepingTaskId == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.HousekeepingTasks
                .FirstOrDefaultAsync(x => x.HousekeepingTaskId == id);

            if (task == null)
                return NotFound();

            var dto = new HousekeepingTaskDto
            {
                HousekeepingTaskId = task.HousekeepingTaskId,
                RoomId = task.RoomId,
                AssignedTo = task.AssignedToStaffId,
                GuestName = task.GuestName,

                TaskType = task.TaskType,
                Priority = task.Priority,
                Status = task.Status,

                TaskDate = task.TaskDate,
                DueDate = task.DueDate,
                Floor = int.TryParse(task.Floor, out var _f) ? _f : (int?)null,

                Description = task.Description,
                Instructions = task.Instructions,
                Notes = task.Notes
            };

            await LoadDropdowns();
            return View(dto);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HousekeepingTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(dto);
            }

            var task = await _context.HousekeepingTasks
                .FirstOrDefaultAsync(x => x.HousekeepingTaskId == dto.HousekeepingTaskId);

            if (task == null)
                return NotFound();

            if (dto.DueDate < dto.TaskDate)
            {
                ModelState.AddModelError("DueDate", "Due date cannot be before task date.");
                await LoadDropdowns();
                return View(dto);
            }

            task.RoomId = dto.RoomId;
            task.AssignedToStaffId = dto.AssignedTo;
            task.GuestName = dto.GuestName;

            task.TaskType = dto.TaskType;
            task.Priority = dto.Priority;
            task.Status = dto.Status;

            task.TaskDate = dto.TaskDate;
            task.DueDate = dto.DueDate;
            task.Floor = dto.Floor.HasValue ? dto.Floor.Value.ToString() : task.Floor;

            task.Description = dto.Description;
            task.Instructions = dto.Instructions;
            task.Notes = dto.Notes;

            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Housekeeping task updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE (soft delete — entity has IsDeleted)
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.HousekeepingTasks
                .FirstOrDefaultAsync(x => x.HousekeepingTaskId == id);

            if (task == null)
                return NotFound();

            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Housekeeping task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // LOAD DROPDOWNS
        // =========================================================

        private async Task LoadDropdowns()
        {
            // Rooms (full entity list used by view)
            ViewBag.Rooms = await _context.Rooms
                .OrderBy(x => x.RoomNumber)
                .ToListAsync();

            // Staff: expose simple Id/Name objects so the view can bind to .Id/.Name
            var staffList = await _context.AppUsers
                .OrderBy(x => x.Username)
                .Select(s => new { Id = s.Id, Name = s.Username })
                .ToListAsync();

            ViewBag.StaffList = staffList;

            // Floors: read distinct floor text values from DB, then parse numeric floors in memory
            var floorTexts = await _context.Rooms
                .Select(x => x.Floor)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct()
                .ToListAsync();

            ViewBag.Floors = floorTexts
                .Select(f => { return int.TryParse(f, out var v) ? (int?)v : null; })
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // Task types: provide default list if none exist in DB for housekeeping.
            ViewBag.TaskTypes = new List<string>
            {
                "RoutineCleaning",
                "DeepCleaning",
                "CheckoutCleaning",
                "MaintenanceCheck",
                "Inspection"
            };
        }
    }
}