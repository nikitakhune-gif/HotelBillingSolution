using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
       

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}
