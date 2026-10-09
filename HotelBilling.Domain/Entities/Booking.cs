using HotelBilling.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBilling.Domain.Entities
{
    public class Booking : BaseEntity
    {
        // ---------- Booking Reference ----------
        [Required]
        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty; // e.g. BK-1042

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ---------- Guest Information ----------
        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GuestEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CountryCode { get; set; } = "+91";

        [Required]
        [StringLength(15)]
        public string GuestPhone { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string GuestType { get; set; } = string.Empty; // Individual, Corporate, VIP, Group, Walk-in

        [Required]
        [StringLength(50)]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string IdProofType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string IdProofNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? GuestAddress { get; set; }

        // ---------- Booking / Stay Details ----------
        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public int TotalNights { get; set; }

        public int Adults { get; set; } = 1;

        public int Children { get; set; }

        [StringLength(30)]
        public string? RoomPreference { get; set; }

        [StringLength(30)]
        public string? BedType { get; set; }

        public string? SpecialRequests { get; set; }

        // ---------- Room Selection ----------
        [Required]
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [StringLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoomType { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Floor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RoomPricePerNight { get; set; }

        // ---------- Pricing ----------
        [Column(TypeName = "decimal(18,2)")]
        public decimal RoomTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraBedCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalServicesCharge { get; set; }

        [StringLength(30)]
        public string? CouponCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; } = 12;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // ---------- Payment ----------
        [StringLength(30)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, UPI, Card, Net Banking, Wallet

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending"; // Paid, Pending, Partial, Refunded

        // ---------- Status ----------
        [StringLength(30)]
        public string BookingStatus { get; set; } = "Confirmed"; // Confirmed, Checked-In, Checked-Out, Cancelled, Reserved
    }
}