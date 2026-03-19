using HotelBilling.Domain.Common;
using HotelBilling.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Entities
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;

        public RoomType RoomType { get; set; }

        public decimal Price { get; set; }

        // Navigation Property (One Room → Many Bills)
        public ICollection<Bill>? Bills { get; set; }
    }
}
