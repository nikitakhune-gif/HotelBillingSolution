using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs.Settings
{
    public class SettingsDto
    {
        public string? SiteName { get; set; }
        public string? CompanyEmail { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class SupportTicketDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSupportTicketDto
    {
        [Required]
        public string Subject { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class UpdateSupportTicketDto
    {
        [Required]
        public string Subject { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class AlertDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public bool IsRead { get; set; }
    }
}
