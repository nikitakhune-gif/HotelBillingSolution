using System.Collections.Generic;

namespace HotelBilling.Application.DTOs.Customer
{
    public class CustomerIndexViewModel
    {
        public IEnumerable<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
        public CustomerSummaryDto Summary { get; set; } = new CustomerSummaryDto();
    }
}
