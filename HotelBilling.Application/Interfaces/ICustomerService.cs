namespace HotelBilling.Application.Interfaces
{
    public interface ICustomerService
    {
        // Create
        Task<int> CreateCustomerAsync(HotelBilling.Application.DTOs.Customer.CustomerDto request);

        // Get All
        Task<IEnumerable<HotelBilling.Application.DTOs.Customer.CustomerDto>> GetAllCustomersAsync();

        // Get By Id
        Task<HotelBilling.Application.DTOs.Customer.CustomerDto?> GetCustomerByIdAsync(int id);

        // Update
        Task<bool> UpdateCustomerAsync(int id, HotelBilling.Application.DTOs.Customer.CustomerDto request);

        // Delete
        Task<bool> DeleteCustomerAsync(int id);
        Task<dynamic> GetAllAsync();
    }
}