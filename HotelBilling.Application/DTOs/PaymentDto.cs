using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs
{
    // ===================================================================
    // Used by: Payment.cshtml -> table rows in "Payment List"
    // ===================================================================
    public class PaymentListItemDto
    {
        public int Id { get; set; }
        public string PaymentCode { get; set; } = string.Empty;   // #PMT-1042
        public string BillNo { get; set; } = string.Empty;     // INV-3301
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerInitials { get; set; } = string.Empty; // for avatar-sm, e.g. "AS"
        public DateTime PaymentDate { get; set; }
        public string Method { get; set; } = string.Empty;        // UPI, Card, Net Banking...
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;        // Success / Pending / Failed
    }

    // ===================================================================
    // Used by: Payment.cshtml -> top stat cards
    // ===================================================================
    public class PaymentStatsDto
    {
        public int TotalPayments { get; set; }
        public int SuccessfulCount { get; set; }
        public int PendingCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalReceived { get; set; }

        public double SuccessfulPercent => TotalPayments == 0 ? 0 : Math.Round(SuccessfulCount * 100.0 / TotalPayments, 2);
        public double PendingPercent => TotalPayments == 0 ? 0 : Math.Round(PendingCount * 100.0 / TotalPayments, 2);
        public double FailedPercent => TotalPayments == 0 ? 0 : Math.Round(FailedCount * 100.0 / TotalPayments, 2);
    }

    // ===================================================================
    // Used by: Payment.cshtml -> right sidebar "Payment Summary"
    // ===================================================================
    public class PaymentSummaryDto
    {
        public int TotalPayments { get; set; }
        public int SuccessfulCount { get; set; }
        public int PendingCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal NetReceived => TotalReceived - TotalRefunded;
        // Additional fields used by services
        public decimal TotalAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public int SuccessCount { get; set; }
        public int RefundedCount { get; set; }
    }

    // ===================================================================
    // Used by: Payment.cshtml -> "Payment Methods" doughnut + legend
    // ===================================================================
    public class PaymentMethodBreakdownDto
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string ColorHex { get; set; } = string.Empty; // e.g. "#2563EB"
    }

    // ===================================================================
    // Used by: Payment.cshtml -> "Recent Transactions" side list
    // ===================================================================
    public class RecentTransactionDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerInitials { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; } // true = money in (arrow-up), false = refund/debit (arrow-down)
    }

    // ===================================================================
    // Used by: NewPayment.cshtml -> invoice dropdown options
    // ===================================================================
    public class InvoiceOptionDto
    {
        public int BillId { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public decimal BillAmount { get; set; }
    }

    // ===================================================================
    // Used by: NewPayment.cshtml -> submitted create form
    // ===================================================================
    public class PaymentCreateDto
    {
        [Required(ErrorMessage = "Please select an invoice.")]
        public int BillId { get; set; }

        /// <summary>Optional override; if null, guest is taken from the invoice.</summary>
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Please select a payment date.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string Method { get; set; } = string.Empty; // card | upi | netbanking | wallet | cash

        [MaxLength(50)]
        public string? TransactionId { get; set; }

        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Populated only when Method == "card".
        // NOTE: raw card number / CVV should NEVER reach the server as-is in a
        // real implementation — collect them client-side and send only a
        // gateway token (e.g. via Stripe.js / Razorpay Checkout). Fields below
        // are placeholders for that token + the last 4 digits for display.
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public string? CardholderName { get; set; }
        public bool SaveCard { get; set; }
        public string? GatewayCardToken { get; set; }
        // Optional external payment identifier
        public string? PaymentId { get; set; }
    }

    // ===================================================================
    // Used by: NewPayment.cshtml -> right sidebar live "Payment Summary"
    // (also returned by the API after computing charges server-side)
    // ===================================================================
    public class PaymentAmountSummaryDto
    {
        public decimal BillAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TaxRatePercent { get; set; } = 12m;
        public decimal TaxAmount => Math.Max(0, BillAmount - Discount + OtherCharges) * TaxRatePercent / 100m;
        public decimal TotalAmount => Math.Max(0, BillAmount - Discount + OtherCharges) + TaxAmount;
    }

    // ===================================================================
    // Response DTO after a successful payment creation
    // ===================================================================
    public class PaymentResultDto
    {
        public int Id { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    // ===================================================================
    // Used for the filter bar at the top of Payment.cshtml
    // ===================================================================
    public class PaymentFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // ===================================================================
    // Generic paged wrapper for the list endpoint
    // ===================================================================
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // ===================================================================
    // Detailed DTO representing a Payment entity
    // ===================================================================
    public class PaymentDto
    {
        public int Id { get; set; }
        public string? PaymentId { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public int BillId { get; set; }
        public string? BillNo { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? TransactionId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public string? CardholderName { get; set; }
        public bool SaveCard { get; set; }
        public string? GatewayCardToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    // ===================================================================
    // Lightweight DTO used by services for list endpoints
    // ===================================================================
    public class PaymentListDto
    {
        public int Id { get; set; }
        public string? PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? BillRef { get; set; }
        public string? TransactionId { get; set; }
    }

    // ===================================================================
    // DTO used to update existing payments
    // ===================================================================
    public class PaymentUpdateDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? Method { get; set; }
        public string? TransactionId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}