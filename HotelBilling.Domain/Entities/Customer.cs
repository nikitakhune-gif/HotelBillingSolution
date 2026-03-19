using HotelBilling.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        // Navigation Property (One Customer → Many Bills)
        public ICollection<Bill>? Bills { get; set; }
    }
}
