using HotelBilling.Domain.Common;
using HotelBilling.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBilling.Domain.Entities
{
    public class Payment : BaseEntity
    {
        /// <summary>Optional external identifier (GUID/string) used by APIs/clients.</summary>
        [StringLength(100)]
        public string? PaymentId { get; set; }

        /// <summary>Human friendly code shown in UI, e.g. "PMT-1042"</summary>
        [Required, MaxLength(20)]
        public string PaymentCode { get; set; } = string.Empty;

        [Required]
        public int BillId { get; set; }

        [ForeignKey(nameof(BillId))]
        public Bill? Bill { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherCharges { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>Final amount actually paid = Amount - Discount + OtherCharges + TaxAmount</summary>
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundedAmount { get; set; }

        [MaxLength(50)]
        public string? TransactionId { get; set; }

        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // ---- Card related (PCI-safe: never store full PAN / CVV) ----
        [MaxLength(4)]
        public string? CardLast4 { get; set; }

        [MaxLength(20)]
        public string? CardBrand { get; set; } // Visa, MasterCard, Rupay, etc.

        [MaxLength(100)]
        public string? CardholderName { get; set; }

        public bool SaveCard { get; set; }

        /// <summary>Token returned by the payment gateway, if a card was saved. Never store raw card data.</summary>
        [MaxLength(200)]
        public string? GatewayCardToken { get; set; }
    }
}
