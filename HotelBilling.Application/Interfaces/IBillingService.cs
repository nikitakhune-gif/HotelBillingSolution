namespace HotelBilling.Application.Interfaces
{
    public interface IBillingService
    {
        // Create Bill
        Task<HotelBilling.Application.DTOs.Bill.BillDto> GenerateBillAsync(HotelBilling.Application.DTOs.Bill.BillDto request);

        // Get All Bills
        Task<IEnumerable<HotelBilling.Application.DTOs.Bill.BillDto>> GetAllAsync();

        // Get Bill By Id
        Task<HotelBilling.Application.DTOs.Bill.BillDto?> GetByIdAsync(int id);

        // Update Payment Status (pass enum int or string handled inside service)
        Task<bool> UpdatePaymentStatusAsync(int billId, int paymentStatus);

        // Delete Bill
        Task<bool> DeleteBillAsync(int id);
    }
}