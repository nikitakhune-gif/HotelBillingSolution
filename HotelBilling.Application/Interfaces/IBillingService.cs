using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.DTOs.Response;

namespace HotelBilling.Application.Interfaces
{
    public interface IBillingService
    {
        // Create Bill
        Task<BillResponse> GenerateBillAsync(CreateBillRequest request);

        // Get All Bills
        Task<IEnumerable<BillResponse>> GetAllAsync();

        // Get Bill By Id
        Task<BillResponse?> GetByIdAsync(int id);

        // Update Payment Status
        Task<bool> UpdatePaymentStatusAsync(int billId, int paymentStatus);

        // Delete Bill
        Task<bool> DeleteBillAsync(int id);
    }
}