using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

// ==========================================================================
// FILE: HousekeepingDtos.cs
// Contains ALL Housekeeping DTOs used across the module:
//   1. HousekeepingDashboardDto  -> Views/Housekeeping/Index.cshtml
//   2. HousekeepingTaskDto       -> Views/Housekeeping/Create.cshtml (+ Edit)
// ==========================================================================

namespace HotelBillingSolution.Application.DTOs.Housekeeping
{
    /// <summary>
    /// Root DTO consumed by Views/Housekeeping/Index.cshtml
    /// </summary>
    public class HousekeepingDashboardDto
    {
        public HousekeepingSummaryDto Summary { get; set; } = new HousekeepingSummaryDto();

        public HousekeepingFilterDto Filters { get; set; } = new HousekeepingFilterDto();

        public List<RoomStatusRowDto> RoomStatusList { get; set; } = new List<RoomStatusRowDto>();

        public List<StaffOnDutyDto> StaffOnDuty { get; set; } = new List<StaffOnDutyDto>();

        public PaginationDto Pagination { get; set; } = new PaginationDto();

        public CleanlinessScoreDto CleanlinessScore { get; set; } = new CleanlinessScoreDto();
    }

    /// <summary>
    /// Top summary cards: Total Rooms, Clean, In Progress, Due for Cleaning, Out of Order.
    /// </summary>
    public class HousekeepingSummaryDto
    {
        public int TotalRooms { get; set; }

        public int CleanRooms { get; set; }
        public decimal CleanPercent { get; set; }

        public int InProgressRooms { get; set; }
        public decimal InProgressPercent { get; set; }

        public int DueForCleaningRooms { get; set; }
        public decimal DueForCleaningPercent { get; set; }

        public int OutOfOrderRooms { get; set; }
        public decimal OutOfOrderPercent { get; set; }
    }

    /// <summary>
    /// Bound to the filter form (GET query string) on the Index view.
    /// </summary>
    public class HousekeepingFilterDto
    {
        public string? Search { get; set; }

        /// <summary>Clean | InProgress | DueForCleaning | OutOfOrder</summary>
        public string? Status { get; set; }

        /// <summary>Low | Medium | High | Urgent</summary>
        public string? Priority { get; set; }

        public int? AssignedTo { get; set; }

        /// <summary>RoutineCleaning | DeepCleaning | CheckoutCleaning | MaintenanceCheck | Inspection</summary>
        public string? TaskType { get; set; }

        public int? Floor { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }

    /// <summary>
    /// One row of the "Room Status List" table.
    /// </summary>
    public class RoomStatusRowDto
    {
        public int HousekeepingTaskId { get; set; }

        public string RoomNo { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public int Floor { get; set; }

        public string? GuestName { get; set; }

        public string? ReservationStatusText { get; set; }

        /// <summary>Clean | InProgress | DueForCleaning | OutOfOrder</summary>
        public string Status { get; set; } = "Clean";

        public string? AssignedTo { get; set; }

        public DateTime? LastCleaned { get; set; }
    }

    /// <summary>
    /// One entry in the "Staff On Duty" sidebar list.
    /// </summary>
    public class StaffOnDutyDto
    {
        public int StaffMemberId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int RoomsAssigned { get; set; }
    }

    /// <summary>
    /// Drives the table pagination + rows-per-page controls.
    /// </summary>
    public class PaginationDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages =>
            PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }

    /// <summary>
    /// Powers the circular "Cleanliness Score" SVG ring in the sidebar.
    /// </summary>
    public class CleanlinessScoreDto
    {
        /// <summary>0 - 100</summary>
        public int Score { get; set; }

        /// <summary>e.g. "Excellent", "Good", "Needs Attention"</summary>
        public string Label { get; set; } = string.Empty;
    }
}

namespace HotelBillingSolution.Models
{
    /// <summary>
    /// Bound to the "Create New Task" form in Views/Housekeeping/Create.cshtml
    /// (also reusable for Edit.cshtml since field names match).
    /// </summary>
    public class HousekeepingTaskDto
    {
        /// <summary>Present only on Edit; ignored/empty on Create.</summary>
        public int HousekeepingTaskId { get; set; }

        [Required(ErrorMessage = "Please select a room.")]
        [Display(Name = "Room No.")]
        public int RoomId { get; set; }

        [Display(Name = "Guest Name")]
        [StringLength(150)]
        public string? GuestName { get; set; }

        [Required(ErrorMessage = "Task date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Task Date")]
        public DateTime TaskDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a task type.")]
        [Display(Name = "Task Type")]
        public string TaskType { get; set; } = string.Empty;

        [Display(Name = "Assigned To")]
        public int? AssignedTo { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a priority.")]
        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public int? Floor { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Instructions { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>Files dropped/browsed in the Attachments card (JPG, PNG, PDF - max 5MB each).</summary>
        public List<IFormFile>? Attachments { get; set; } = new List<IFormFile>();
    }
}
