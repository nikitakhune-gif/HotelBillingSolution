
using HotelBilling.Application.DTOs.Booking;
using HotelBilling.Application.DTOs;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelBilling.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Room> _roomRepo;
        private readonly IRepository<Bill> _billRepo;
        private readonly IRepository<BillItem> _billItemRepo;
        private readonly IUnitOfWork _uow;

        public BookingService(
            IRepository<Booking> bookingRepo,
            IRepository<Customer> customerRepo,
            IRepository<Payment> paymentRepo,
            IRepository<Room> roomRepo,
            IRepository<Bill> billRepo,
            IRepository<BillItem> billItemRepo,
            IUnitOfWork uow)
        {
            _bookingRepo = bookingRepo;
            _customerRepo = customerRepo;
            _paymentRepo = paymentRepo;
            _roomRepo = roomRepo;
            _billRepo = billRepo;
            _billItemRepo = billItemRepo;
            _uow = uow;
        }

        public async Task<int> CreateBookingAsync(BookingCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (dto.CheckOutDate <= dto.CheckInDate)
                throw new ArgumentException("Check-out must be after check-in");

            // NOTE: Wrapped in _uow.ExecuteInTransactionAsync instead of manual
            // BeginTransactionAsync/Commit/Rollback. The DbContext is configured
            // with EnableRetryOnFailure (SqlServerRetryingExecutionStrategy), which
            // refuses to run inside a user-initiated transaction. Using the
            // execution-strategy-aware transaction wrapper fixes the
            // "does not support user-initiated transactions" failure that was
            // silently blocking every booking. Commit/rollback are handled
            // automatically by ExecuteInTransactionAsync.
            var bookingEntity = await _uow.ExecuteInTransactionAsync(async () =>
            {
                // Find or create customer
                Customer? customer = null;
                if (dto.CustomerId > 0)
                {
                    customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
                    if (customer == null) throw new Exception("Customer not found");
                }
                else if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var found = (await _customerRepo.FindAsync(c => c.Email == dto.Email)).FirstOrDefault();
                    if (found != null) customer = found;
                }

                if (customer == null)
                {
                    customer = new Customer
                    {
                        FirstName = dto.GuestName ?? dto.Email ?? "Guest",
                        LastName = string.Empty,
                        Email = dto.Email ?? string.Empty,
                        MobileNumber = dto.PhoneNumber ?? string.Empty,
                        Username = dto.Email ?? dto.GuestName ?? Guid.NewGuid().ToString(),
                        PasswordHash = Guid.NewGuid().ToString()
                    };

                    await _customerRepo.AddAsync(customer);
                }

                // Check room
                var room = await _roomRepo.GetByIdAsync(dto.RoomId);
                if (room == null) throw new Exception("Selected room not found");
                if (!string.Equals(room.Availability, "Available", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Selected room is not available");

                // Nights
                var nights = (int)(dto.CheckOutDate - dto.CheckInDate).TotalDays;
                if (nights <= 0) throw new Exception("Invalid number of nights");

                var pricePerNight = room.PricePerNight ?? 0m;
                var subTotal = Math.Round(pricePerNight * nights, 2);

                // Coupon
                decimal discountPercent = 0m;
                if (!string.IsNullOrWhiteSpace(dto.CouponCode))
                {
                    var coupon = await ApplyCouponAsync(dto.CouponCode, subTotal, cancellationToken);
                    if (!coupon.valid) throw new Exception(coupon.message);
                    discountPercent = coupon.discountPercent;
                }

                var discountAmount = Math.Round(subTotal * (discountPercent / 100m), 2);

                var taxPercent = room.Tax ?? 12m;
                var taxable = Math.Max(0, subTotal - discountAmount);
                var taxAmount = Math.Round(taxable * (taxPercent / 100m), 2);

                var serviceCharge = 0m;
                var total = Math.Round(Math.Max(0, taxable + taxAmount + serviceCharge), 2);
                var advance = Math.Round(total * 0.30m, 2);
                var due = Math.Round(total - advance, 2);

                // Create booking
                var booking = new Booking
                {
                    BookingNumber = string.IsNullOrWhiteSpace(dto.BookingNumber) ? $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}" : dto.BookingNumber,
                    Customer = customer,
                    GuestName = dto.GuestName ?? customer.FirstName,
                    GuestEmail = dto.Email ?? customer.Email,
                    CountryCode = "+91",
                    GuestPhone = dto.PhoneNumber ?? customer.MobileNumber,
                    IdProofType = dto.IdProofType ?? string.Empty,
                    IdProofNumber = dto.IdProofNumber ?? string.Empty,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    TotalNights = nights,
                    Adults = dto.Adults,
                    Children = dto.Children,
                    RoomId = room.Id,
                    RoomNumber = room.RoomNumber,
                    RoomType = room.RoomType,
                    Floor = room.Floor,
                    RoomPricePerNight = pricePerNight,
                    RoomTotal = subTotal,
                    CouponCode = dto.CouponCode,
                    DiscountAmount = discountAmount,
                    TaxPercent = taxPercent,
                    TaxAmount = taxAmount,
                    TotalAmount = total,
                    AdvanceAmount = advance,
                    RemainingAmount = due,
                    PaymentMethod = dto.PaymentMethod ?? "Cash",
                    PaymentStatus = HotelBilling.Domain.Enums.PaymentStatus.Pending.ToString(),
                    BookingStatus = dto.BookingStatus ?? "Confirmed",
                    SpecialRequests = dto.SpecialRequests,
                    CreatedDate = DateTime.UtcNow
                };

                await _bookingRepo.AddAsync(booking);

                // Create bill
                var bill = new Bill
                {
                    BillNumber = $"B-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    BillDate = DateTime.UtcNow,
                    Customer = customer,
                    RoomId = room.Id,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    Nights = nights,
                    SubTotal = subTotal,
                    DiscountPercent = discountPercent,
                    DiscountAmount = discountAmount,
                    TaxPercent = taxPercent,
                    TaxAmount = taxAmount,
                    ServiceCharge = serviceCharge,
                    TotalAmount = total,
                    PaymentMethod = dto.PaymentMethod,
                    AdvanceReceived = advance,
                    PaidAmount = advance,
                    DueAmount = due,
                    PaymentDate = DateTime.UtcNow,
                    PaymentStatus = HotelBilling.Domain.Enums.PaymentStatus.Paid.ToString(),
                    BillStatus = "Due"
                };

                await _billRepo.AddAsync(bill);

                // Bill item
                var billItem = new BillItem
                {
                    Bill = bill,
                    ItemName = string.IsNullOrWhiteSpace(room.RoomName) ? room.RoomNumber : room.RoomName,
                    Quantity = nights,
                    Rate = pricePerNight,
                    Amount = pricePerNight * nights
                };

                await _billItemRepo.AddAsync(billItem);

                // Payment record for advance
                if (advance > 0)
                {
                    // Map payment method string to enum
                    HotelBilling.Domain.Enums.PaymentMethod pmEnum = HotelBilling.Domain.Enums.PaymentMethod.Cash;
                    var pmStr = (dto.PaymentMethod ?? "Cash").Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
                    if (string.Equals(pmStr, nameof(HotelBilling.Domain.Enums.PaymentMethod.UPI), StringComparison.OrdinalIgnoreCase)) pmEnum = HotelBilling.Domain.Enums.PaymentMethod.UPI;
                    else if (string.Equals(pmStr, nameof(HotelBilling.Domain.Enums.PaymentMethod.Card), StringComparison.OrdinalIgnoreCase)) pmEnum = HotelBilling.Domain.Enums.PaymentMethod.Card;
                    else if (string.Equals(pmStr, nameof(HotelBilling.Domain.Enums.PaymentMethod.NetBanking), StringComparison.OrdinalIgnoreCase) || string.Equals(pmStr, "NetBanking", StringComparison.OrdinalIgnoreCase)) pmEnum = HotelBilling.Domain.Enums.PaymentMethod.NetBanking;
                    else if (string.Equals(pmStr, nameof(HotelBilling.Domain.Enums.PaymentMethod.Wallet), StringComparison.OrdinalIgnoreCase)) pmEnum = HotelBilling.Domain.Enums.PaymentMethod.Wallet;

                    var payment = new Payment
                    {
                        Bill = bill,
                        Customer = customer,
                        PaymentDate = DateTime.UtcNow,
                        PaymentMethod = pmEnum,
                        Amount = advance,
                        Discount = discountAmount,
                        OtherCharges = 0m,
                        TaxAmount = taxAmount,
                        TotalAmount = total,
                        PaymentStatus = HotelBilling.Domain.Enums.PaymentStatus.Paid
                    };
                    payment.PaymentCode = $"PMT-{DateTime.UtcNow:yyyyMMddHHmmss}";
                    await _paymentRepo.AddAsync(payment);
                }

                // Update room availability
                room.Availability = "Occupied";
                _roomRepo.Update(room);

                // Return the booking entity. UnitOfWork.ExecuteInTransactionAsync
                // will call SaveChangesAsync AFTER this delegate returns, and
                // the returned entity reference will have its Id populated by
                // SaveChanges. Returning the entity ensures the caller sees the
                // generated primary key.
                return booking;
            }, cancellationToken);

            return bookingEntity.Id;
        }

        public async Task<HotelBilling.Application.DTOs.PagedResultDto<BookingListItemDto>> GetPagedBookingsAsync(BookingFilterDto filter, CancellationToken cancellationToken = default)
        {
            var all = await _bookingRepo.GetAllAsync();

            var q = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.BookingNumber))
                q = q.Where(x => x.BookingNumber.Contains(filter.BookingNumber, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.GuestName))
                q = q.Where(x => x.Customer != null && ((x.Customer.FirstName + " " + x.Customer.LastName).Contains(filter.GuestName, StringComparison.OrdinalIgnoreCase) || x.GuestName.Contains(filter.GuestName, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrWhiteSpace(filter.RoomNumber))
                q = q.Where(x => x.Room != null && x.Room.RoomNumber.Contains(filter.RoomNumber, StringComparison.OrdinalIgnoreCase));

            if (filter.BookingStatus.HasValue)
                q = q.Where(x => x.BookingStatus == filter.BookingStatus.Value.ToString());

            // Sorting
            q = filter.SortDir?.ToLower() == "asc" ? q.OrderBy(x => x.CreatedDate) : q.OrderByDescending(x => x.CreatedDate);

            var total = q.Count();

            var items = q.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

            var dtoItems = items.Select(b => new BookingListItemDto
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                GuestName = b.Customer != null ? b.Customer.FirstName + " " + b.Customer.LastName : b.GuestName,
                RoomNumber = b.Room != null ? b.Room.RoomNumber : string.Empty,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalNights = b.TotalNights,
                Adults = b.Adults,
                Children = b.Children,
                BookingStatus = b.BookingStatus,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                CreatedDate = b.CreatedDate
            }).ToList();

            return new HotelBilling.Application.DTOs.PagedResultDto<BookingListItemDto>
            {
                Items = dtoItems,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<BookingSummaryDto> GetBookingSummaryAsync(CancellationToken cancellationToken = default)
        {
            var all = await _bookingRepo.GetAllAsync();
            var today = DateTime.UtcNow.Date;

            var todaysBookings = all.Count(x => x.CreatedDate.Date == today);
            var checkIns = all.Count(x => x.CheckInDate.Date == today);
            var checkOuts = all.Count(x => x.CheckOutDate.Date == today);
            var pending = all.Count(x => x.BookingStatus == HotelBilling.Domain.Enums.BookingStatus.Reserved.ToString() || x.BookingStatus == HotelBilling.Domain.Enums.BookingStatus.Confirmed.ToString());

            return new HotelBilling.Application.DTOs.Booking.BookingSummaryDto
            {
                TodaysBookings = todaysBookings,
                CheckInsToday = checkIns,
                CheckOutsToday = checkOuts,
                PendingBookings = pending
            };
        }

        public async Task<DTOs.Room.RoomDto[]> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, string roomType, int? floor, int adults = 1, int children = 0, CancellationToken cancellationToken = default)
        {
            var rooms = await _roomRepo.GetAllAsync();

            var q = rooms.AsQueryable();

            // Only rooms marked as Available
            q = q.Where(r => r.Availability != null && r.Availability.Equals("Available", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(roomType))
                q = q.Where(r => r.RoomType != null && r.RoomType.Equals(roomType, StringComparison.OrdinalIgnoreCase));

            if (floor != null)
                q = q.Where(r => r.Floor != null && r.Floor.Equals(floor.ToString(), StringComparison.OrdinalIgnoreCase));

            // Respect capacity
            if (adults > 0)
                q = q.Where(r => (r.CapacityAdults ?? 0) >= adults);
            if (children > 0)
                q = q.Where(r => (r.CapacityChildren ?? int.MaxValue) >= children);

            var list = q.Select(r => new DTOs.Room.RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomName = r.RoomName,
                RoomType = r.RoomType,
                Floor = r.Floor,
                PricePerNight = r.PricePerNight,
                WeekendPrice = r.WeekendPrice,
                ExtraPersonCharge = r.ExtraPersonCharge,
                Discount = r.Discount,
                Tax = r.Tax,
                Availability = r.Availability,
                IsActive = r.IsActive,
                CapacityAdults = r.CapacityAdults,
                CapacityChildren = r.CapacityChildren,
                BedType = r.BedType,
                ImagePaths = r.ImagePaths,
                CreatedDate = r.CreatedDate
            }).ToArray();

            return list;
        }

        public async Task<(bool valid, decimal discountPercent, string message)> ApplyCouponAsync(string code, decimal roomTotal, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, 0, "Coupon code is required");

            // Simple demo: 'WELCOME10' => 10%
            if (code.Equals("WELCOME10", StringComparison.OrdinalIgnoreCase))
            {
                return (true, 10m, "10% off applied");
            }

            return (false, 0, "Invalid coupon code");
        }

        public async Task<BookingListItemDto?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var b = await _bookingRepo.GetByIdAsync(id);
            if (b == null) return null;

            return new BookingListItemDto
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                GuestName = b.Customer != null ? b.Customer.FirstName + " " + b.Customer.LastName : b.GuestName,
                RoomNumber = b.Room != null ? b.Room.RoomNumber : string.Empty,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalNights = b.TotalNights,
                Adults = b.Adults,
                Children = b.Children,
                BookingStatus = b.BookingStatus,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                CreatedDate = b.CreatedDate
            };
        }
    }
}
