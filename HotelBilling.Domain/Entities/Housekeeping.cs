using HotelBilling.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBillingSolution.Domain.Entities
{
    /// <summary>
    /// Database entity for a single housekeeping task against a room.
    /// Rows here feed both the "Room Status List" table on the dashboard
    /// and the Create/Edit task form.
    /// </summary>
    [Table("HousekeepingTasks")]
    public class HousekeepingTask
    {
        [Key]
        public int HousekeepingTaskId { get; set; }

        // ---------- Room / Reservation link ----------

        [Required]
        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        /// <summary>Optional link to the active booking, if any (drives GuestName/ReservationStatusText on the dashboard).</summary>
        public int? BookingId { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual HotelBilling.Domain.Entities.Booking? Booking { get; set; }

        [StringLength(150)]
        public string? GuestName { get; set; }

        // ---------- Scheduling ----------

        [Required]
        public DateTime TaskDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? LastCleaned { get; set; }

        // ---------- Classification ----------

        /// <summary>RoutineCleaning | DeepCleaning | CheckoutCleaning | MaintenanceCheck | Inspection</summary>
        [Required]
        [StringLength(50)]
        public string TaskType { get; set; } = string.Empty;

        /// <summary>Low | Medium | High | Urgent</summary>
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>Clean | InProgress | DueForCleaning | OutOfOrder (dashboard) — Pending | InProgress | Completed | Overdue (task form)</summary>
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        // Floor is stored as text on Room (e.g. "1", "Ground").
        // Keep task-level floor as a nullable string to avoid creating
        // duplicate/reserved types and to match existing Room.Floor.
        public string? Floor { get; set; }

        // ---------- Staff assignment ----------

        public int? AssignedToStaffId { get; set; }

        // The project models staff users using AppUser. Do not create a
        // duplicate StaffMember type — reuse AppUser here.
        [ForeignKey(nameof(AssignedToStaffId))]
        public virtual HotelBilling.Domain.Entities.AppUser? AssignedToStaff { get; set; }

        // ---------- Free text ----------

        [Required]
        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Instructions { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // ---------- Attachments ----------

        public virtual ICollection<HousekeepingTaskAttachment> Attachments { get; set; }
            = new List<HousekeepingTaskAttachment>();

        // ---------- Audit ----------

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// One uploaded file (JPG, PNG or PDF, max 5MB) attached to a HousekeepingTask.
    /// </summary>
    [Table("HousekeepingTaskAttachments")]
    public class HousekeepingTaskAttachment
    {
        [Key]
        public int HousekeepingTaskAttachmentId { get; set; }

        [Required]
        public int HousekeepingTaskId { get; set; }

        [ForeignKey(nameof(HousekeepingTaskId))]
        public virtual HousekeepingTask? HousekeepingTask { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(20)]
        public string FileExtension { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}