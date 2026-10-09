using System;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs.Customer
{
    public class CustomerDto
    {
        // Update/Read sathi lagto, Create karताna 0 rahil
        public int Id { get; set; }

        // ---------- Personal Information ----------
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        // CreatedDate from BaseEntity — used for "New Customers" summary
        public DateTime CreatedDate { get; set; }

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

        // Create वेळी plain password ithun yeईल, Update/Read वेळी empty theवा
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

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

        // ---------- Read-only helper ----------
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}