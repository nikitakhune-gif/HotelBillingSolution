using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;
using HotelBilling.Application.DTOs.Dashboard;

namespace HotelBilling.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Room> _roomRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Payment> _paymentRepo;

        public DashboardService(
            IRepository<Room> roomRepo,
            IRepository<Customer> customerRepo,
            IRepository<Booking> bookingRepo,
            IRepository<Payment> paymentRepo)
        {
            _roomRepo = roomRepo;
            _customerRepo = customerRepo;
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var vm = new DashboardDto();

            var rooms = (await _roomRepo.GetAllAsync()).ToList();
            var customers = (await _customerRepo.GetAllAsync()).ToList();
            var bookings = (await _bookingRepo.GetAllAsync()).ToList();
            var payments = (await _paymentRepo.GetAllAsync()).ToList();

            // Totals
            vm.TotalRooms = rooms.Count;
            vm.AvailableRooms = rooms.Count(r => string.Equals(r.Availability, "Available", StringComparison.OrdinalIgnoreCase));
            vm.TotalCustomers = customers.Count;
            vm.TotalBookings = bookings.Count;

            // Today's revenue
            var today = DateTime.UtcNow.Date;
            vm.TodaysRevenue = payments.Where(p => p.PaymentDate.Date == today).Sum(p => p.TotalAmount);

            // Booking summary
            vm.TodaysBookings = bookings.Count(b => b.CreatedDate.Date == today);
            vm.CheckInsToday = bookings.Count(b => b.CheckInDate.Date == today);
            vm.CheckOutsToday = bookings.Count(b => b.CheckOutDate.Date == today);

            // Room status counts
            var statusGroups = rooms.GroupBy(r => string.IsNullOrWhiteSpace(r.Availability) ? "Unknown" : r.Availability)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            // Ensure keys for common statuses
            string[] statuses = new[] { "Occupied", "Available", "Reserved", "Maintenance", "Unknown" };
            foreach (var s in statuses)
            {
                statusGroups.TryGetValue(s, out var count);
                vm.RoomStatusCounts[s] = count;
            }

            // Occupancy percent
            var occupied = statusGroups.ContainsKey("Occupied") ? statusGroups["Occupied"] : 0;
            vm.OccupancyPercent = vm.TotalRooms > 0 ? Math.Round((decimal)occupied * 100m / vm.TotalRooms, 0) : 0;

            // Revenue overview - last 12 months
            var from = DateTime.UtcNow.AddMonths(-11);
            var paymentsLastYear = payments.Where(p => p.PaymentDate >= from).ToList();

            var revGroups = paymentsLastYear
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(x => x.TotalAmount) })
                .ToList();

            // Create 12-month series (fill missing months)
            var months = Enumerable.Range(0, 12).Select(i => from.AddMonths(i)).ToList();
            foreach (var m in months)
            {
                var found = revGroups.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month);
                vm.RevenueLabels.Add(m.ToString("MMM yy"));
                vm.RevenueSeries.Add(found != null ? found.Amount : 0m);
            }

            // Recent bookings (latest 5) - avoid N+1 by bulk lookup
            var recent = bookings.OrderByDescending(b => b.CreatedDate).Take(5).ToList();
            var customerIds = recent.Select(r => r.CustomerId).Distinct().ToList();
            var roomIds = recent.Select(r => r.RoomId).Distinct().ToList();

            var customerMap = customers.Where(c => customerIds.Contains(c.Id)).ToDictionary(c => c.Id);
            var roomMap = rooms.Where(r => roomIds.Contains(r.Id)).ToDictionary(r => r.Id);

            foreach (var b in recent)
            {
                var name = b.Customer != null ? (b.Customer.FirstName + " " + b.Customer.LastName).Trim() : (customerMap.ContainsKey(b.CustomerId) ? (customerMap[b.CustomerId].FirstName + " " + customerMap[b.CustomerId].LastName).Trim() : b.GuestName);
                var avatar = customerMap.ContainsKey(b.CustomerId) && !string.IsNullOrWhiteSpace(customerMap[b.CustomerId].ProfilePhotoPath)
                    ? customerMap[b.CustomerId].ProfilePhotoPath
                    : "/img/maleavtar.png"; // fallback

                vm.RecentBookings.Add(new RecentBookingDto
                {
                    Id = b.Id,
                    GuestName = name,
                    BookingNumber = b.BookingNumber,
                    RoomLabel = (roomMap.ContainsKey(b.RoomId) ? roomMap[b.RoomId].RoomType : b.RoomType) + " · " + b.RoomNumber,
                    CheckInDate = b.CheckInDate,
                    BookingStatus = b.BookingStatus,
                    AvatarUrl = avatar
                });
            }

            // Top customers - by payments total
            var topGroups = payments.GroupBy(p => p.CustomerId)
                .Select(g => new { CustomerId = g.Key, Total = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            foreach (var tg in topGroups)
            {
                var cust = customers.FirstOrDefault(c => c.Id == tg.CustomerId);
                var name = cust != null ? (cust.FirstName + " " + cust.LastName).Trim() : "Guest";
                var email = cust?.Email ?? string.Empty;
                var avatar = cust != null && !string.IsNullOrWhiteSpace(cust.ProfilePhotoPath) ? cust.ProfilePhotoPath : "/img/maleavtar.png";

                vm.TopCustomers.Add(new TopCustomerDto
                {
                    CustomerId = tg.CustomerId,
                    Name = name,
                    Email = email,
                    TotalSpent = tg.Total,
                    StayCount = tg.Count,
                    AvatarUrl = avatar
                });
            }

            return vm;
        }
    }
}
