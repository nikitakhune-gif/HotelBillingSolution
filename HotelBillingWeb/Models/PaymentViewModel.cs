using HotelBilling.Application.DTOs;
using HotelBilling.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace HotelBillingWeb.Models
{
    public class PaymentViewModel
    {
        // For Create/Edit
        public PaymentCreateDto Payment { get; set; } = new PaymentCreateDto();

        // For Listing
        public IEnumerable<PaymentListDto> Payments { get; set; } = new List<PaymentListDto>();

        // Summary
        public PaymentSummaryDto Summary { get; set; } = new PaymentSummaryDto();

        // Dropdowns
        public IEnumerable<SelectListItem> InvoiceList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> GuestList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PaymentMethodList { get; set; } = new List<SelectListItem>();

        // Filters
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
