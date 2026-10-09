using System;
using HotelBilling.Domain.Common;

namespace HotelBilling.Domain.Entities
{
    public class AppUser : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
