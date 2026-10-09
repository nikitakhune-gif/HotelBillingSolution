namespace HotelBilling.Application.DTOs.Customer
{
    public class CustomerSummaryDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int VipCustomers { get; set; }
    }
}
