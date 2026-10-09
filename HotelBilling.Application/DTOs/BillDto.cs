using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs.Bill
{
    public class BillDto
    {
        
        public int Id { get; set; }

        
        [Required]
        [StringLength(30)]
        public string BillNumber { get; set; } = string.Empty;

        public DateTime BillDate { get; set; } = DateTime.Now;

        
        [Required]
        public int CustomerId { get; set; }

        
        public string? CustomerName { get; set; }

        [Required]
        public int RoomId { get; set; }

        // Read response sathi (list page varचे "Room No")
        public string? RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int Nights { get; set; }

        // ---------- Charges ----------
        public decimal SubTotal { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        public decimal DiscountAmount { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ServiceCharge { get; set; }

        public decimal TotalAmount { get; set; }

        // ---------- Payment Details ----------
        [StringLength(30)]
        public string? PaymentMethod { get; set; }

        public decimal AdvanceReceived { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal DueAmount { get; set; }

        [StringLength(50)]
        public string? TransactionNumber { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        // ---------- Bill Status ----------
        [StringLength(30)]
        public string BillStatus { get; set; } = "Due";

        // ---------- Notes ----------
        public string? Notes { get; set; }

        // ---------- Line Items ----------
        public List<BillItemDto> Items { get; set; } = new();
    }

    public class BillItemDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ItemName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; }

        // Server-side (Qty * Rate) calculate करूनच send karaycha, pan display sathi ithe pan thevla
        public decimal Amount { get; set; }
    }
}