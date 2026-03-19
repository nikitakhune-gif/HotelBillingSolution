using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Application.DTOs.Request
{
    public class CreateBillRequest
    {
        public int CustomerId { get; set; }

        public int RoomId { get; set; }

        // Optional: number of days stayed (for calculation)
        public int NumberOfDays { get; set; }

        // Additional fields used by some tests/controllers (optional)
        public int RoomNumber { get; set; }
        public decimal RoomCharge { get; set; }
        public decimal FoodCharge { get; set; }
        public decimal OtherCharges { get; set; }
    }
}
