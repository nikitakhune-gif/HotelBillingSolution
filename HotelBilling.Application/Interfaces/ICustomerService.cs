using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.DTOs.Response;

namespace HotelBilling.Application.Interfaces
{
    public interface ICustomerService
    {
        // Create
        Task<int> CreateCustomerAsync(CreateCustomerRequest request);

        // Get All
        Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync();

        // Get By Id
        Task<CustomerResponse?> GetCustomerByIdAsync(int id);

        // Update
        Task<bool> UpdateCustomerAsync(int id, CreateCustomerRequest request);

        // Delete
        Task<bool> DeleteCustomerAsync(int id);
        Task<dynamic> GetAllAsync();
    }
}