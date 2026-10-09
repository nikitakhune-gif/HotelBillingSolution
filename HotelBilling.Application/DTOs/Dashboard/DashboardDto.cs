using System;
using System.Collections.Generic;

namespace HotelBilling.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TodaysRevenue { get; set; }
        public int TotalBookings { get; set; }

        public List<decimal> RevenueSeries { get; set; } = new();
        public List<string> RevenueLabels { get; set; } = new();

        public Dictionary<string,int> RoomStatusCounts { get; set; } = new();

        public decimal OccupancyPercent { get; set; }

        public int CheckInsToday { get; set; }
        public int CheckOutsToday { get; set; }
        public int TodaysBookings { get; set; }

        public List<RecentBookingDto> RecentBookings { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
    }

    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public string RoomLabel { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int StayCount { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
