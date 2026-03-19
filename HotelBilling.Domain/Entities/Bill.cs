using HotelBilling.Domain.Common;
using HotelBilling.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Entities
{
    public class Bill : BaseEntity
    {
        public int CustomerId { get; set; }

        public int RoomId { get; set; }

        public decimal Amount { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        // Navigation Properties
        public Customer? Customer { get; set; }

        public Room? Room { get; set; }
    }
}
