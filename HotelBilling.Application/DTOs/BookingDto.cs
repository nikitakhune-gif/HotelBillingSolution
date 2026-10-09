using System;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs.Booking
{
    public class BookingDto
    {
        // Update/Read sathi lagto, Create karताna 0 rahil
        public int Id { get; set; }

        [StringLength(20)]
        public string BookingNumber { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ---------- Guest Information ----------
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
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
        public string GuestType { get; set; } = string.Empty;

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

        [Range(1, 20)]
        public int Adults { get; set; } = 1;

        [Range(0, 20)]
        public int Children { get; set; }

        [StringLength(30)]
        public string? RoomPreference { get; set; }

        [StringLength(30)]
        public string? BedType { get; set; }

        public string? SpecialRequests { get; set; }

        // ---------- Room Selection ----------
        [Required]
        public int RoomId { get; set; }

        [StringLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoomType { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Floor { get; set; }

        public decimal RoomPricePerNight { get; set; }

        // ---------- Pricing ----------
        public decimal RoomTotal { get; set; }

        public decimal ExtraBedCharge { get; set; }

        public decimal AdditionalServicesCharge { get; set; }

        [StringLength(30)]
        public string? CouponCode { get; set; }

        public decimal DiscountAmount { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; } = 12;

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        // ---------- Payment ----------
        [StringLength(30)]
        public string PaymentMethod { get; set; } = "Cash";

        public decimal AdvanceAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        // ---------- Status ----------
        [StringLength(30)]
        public string BookingStatus { get; set; } = "Confirmed";
    }

    // DTO used to create a booking (submitted from UI)
    public class BookingCreateDto
    {
        // Optional: if provided, use existing customer; otherwise service will find by email or create
        public int CustomerId { get; set; }
        // Payment method posted from the UI (e.g. Cash, UPI, Card, Net Banking, Wallet)
        public string? PaymentMethod { get; set; } = "Cash";
        public string BookingNumber { get; set; } = string.Empty;
        // removed duplicate CustomerId
        public string GuestName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? IdProofType { get; set; }
        public string? IdProofNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int RoomId { get; set; }
        public int Adults { get; set; } = 1;
        public int Children { get; set; }
        public string? RoomPreference { get; set; }
        public string? BedType { get; set; }
        public string? SpecialRequests { get; set; }
        public string? BookingStatus { get; set; }
        public string? CouponCode { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // DTO used for list items
    public class BookingListItemDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int TotalNights { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    // DTO used for filtering/paging
    public class BookingFilterDto
    {
        public string? BookingNumber { get; set; }
        public string? GuestName { get; set; }
        public string? RoomNumber { get; set; }
        public string? SortDir { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public HotelBilling.Domain.Enums.BookingStatus? BookingStatus { get; set; }
    }

    public class BookingSummaryDto
    {
        public int TodaysBookings { get; set; }
        public int CheckInsToday { get; set; }
        public int CheckOutsToday { get; set; }
        public int PendingBookings { get; set; }
    }

    // ViewModel used by Views/Booking/Index.cshtml
    public class BookingIndexViewModel
    {
        public IEnumerable<BookingListItemDto> Bookings { get; set; } = Enumerable.Empty<BookingListItemDto>();
        public BookingFilterDto Filter { get; set; } = new BookingFilterDto();
        public BookingSummaryDto Summary { get; set; } = new BookingSummaryDto();
        public int TotalCount { get; set; }

        // Recent items shown in the right panel
        public IEnumerable<BookingListItemDto> RecentBookings { get; set; } = Enumerable.Empty<BookingListItemDto>();
        public IEnumerable<BookingListItemDto> UpcomingCheckIns { get; set; } = Enumerable.Empty<BookingListItemDto>();
        public IEnumerable<BookingListItemDto> UpcomingCheckOuts { get; set; } = Enumerable.Empty<BookingListItemDto>();

        // Simple payment item used by the right panel — only includes fields needed by the view
        public IEnumerable<PaymentListItemDto> RecentPayments { get; set; } = Enumerable.Empty<PaymentListItemDto>();
    }

    public class PaymentListItemDto
    {
        public string BookingNumber { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}