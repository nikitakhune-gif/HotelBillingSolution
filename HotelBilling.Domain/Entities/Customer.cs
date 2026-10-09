using HotelBilling.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Domain.Entities
{
    public class Customer : BaseEntity
    {
        // ---------- Personal Information ----------
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(50)]
        public string? Nationality { get; set; }

        public string? ProfilePhotoPath { get; set; }

        // ---------- Account Information ----------
        [StringLength(30)]
        public string CustomerType { get; set; } = "Regular";

        [StringLength(30)]
        public string Membership { get; set; } = "Silver";

        [StringLength(30)]
        public string Status { get; set; } = "Active";

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // ---------- Address Information ----------
        [StringLength(150)]
        public string? AddressLine1 { get; set; }

        [StringLength(150)]
        public string? AddressLine2 { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        // ---------- Identity Information ----------
        [StringLength(30)]
        public string? IdProofType { get; set; }

        [StringLength(50)]
        public string? IdNumber { get; set; }

        [StringLength(100)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? Occupation { get; set; }

        [StringLength(30)]
        public string? GstNumber { get; set; }

        // ---------- Emergency Contact ----------
        [StringLength(100)]
        public string? EmergencyContactPerson { get; set; }

        [StringLength(50)]
        public string? EmergencyRelationship { get; set; }

        [StringLength(15)]
        public string? EmergencyMobile { get; set; }

        [StringLength(15)]
        public string? EmergencyAltMobile { get; set; }

        // ---------- Additional Information ----------
        public string? CustomerNotes { get; set; }

        public string? SpecialRequirements { get; set; }

        [StringLength(50)]
        public string? PreferredRoomType { get; set; }

        [StringLength(30)]
        public string? MealPreference { get; set; }

        // ---------- Navigation Property (One Customer → Many Bills) ----------
        public ICollection<Bill>? Bills { get; set; }
    }
}