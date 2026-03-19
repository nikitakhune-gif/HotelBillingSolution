using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Application.DTOs.Response
{
    public class BillResponse
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int RoomId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        // Additional properties used by controller/tests
        public decimal TotalAmount { get; set; }
    }
}
