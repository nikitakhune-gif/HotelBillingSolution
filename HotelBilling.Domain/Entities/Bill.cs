using HotelBilling.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBilling.Domain.Entities
{
    public class Bill : BaseEntity
    {
        // ---------- Invoice Info ----------
        [Required]
        [StringLength(30)]
        public string BillNumber { get; set; } = string.Empty;

        public DateTime BillDate { get; set; } = DateTime.Now;

        // ---------- Guest & Booking Details ----------
        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int Nights { get; set; }

        // ---------- Charges ----------
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // ---------- Payment Details ----------
        [StringLength(30)]
        public string? PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceReceived { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; }

        [StringLength(50)]
        public string? TransactionNumber { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending"; // Paid, Partial, Pending

        // ---------- Bill Status ----------
        [StringLength(30)]
        public string BillStatus { get; set; } = "Due"; // Completed, Checked Out, Due

        // ---------- Notes ----------
        public string? Notes { get; set; }

        // ---------- Navigation ----------
        public ICollection<BillItem>? Items { get; set; }
    }

    public class BillItem : BaseEntity
    {
        [Required]
        public int BillId { get; set; }
        public Bill? Bill { get; set; }

        [Required]
        [StringLength(150)]
        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}