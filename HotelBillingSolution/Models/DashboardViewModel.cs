using System;
using System.Collections.Generic;

namespace HotelBillingSolution.Models
{
    public class DashboardViewModel
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TodaysRevenue { get; set; }
        public int TotalBookings { get; set; }

        // Revenue chart
        public List<decimal> RevenueSeries { get; set; } = new();
        public List<string> RevenueLabels { get; set; } = new();

        // Room status counts
        public Dictionary<string, int> RoomStatusCounts { get; set; } = new();

        public decimal OccupancyPercent { get; set; }

        // Summary numbers
        public int CheckInsToday { get; set; }
        public int CheckOutsToday { get; set; }
        public int TodaysBookings { get; set; }

        // Recent bookings and top customers
        public List<RecentBookingVm> RecentBookings { get; set; } = new();
        public List<TopCustomerVm> TopCustomers { get; set; } = new();
    }

    public class RecentBookingVm
    {
        public int Id { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public string RoomLabel { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class TopCustomerVm
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int StayCount { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
