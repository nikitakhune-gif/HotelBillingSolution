using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.DTOs.Response;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;


namespace HotelBilling.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IRepository<Customer> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        // CREATE
        public async Task<int> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var customer = new Customer
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone
            };

            await _repo.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return customer.Id;
        }

        // GET ALL
        public async Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync()
        {
            var customers = await _repo.GetAllAsync();

            return customers.Select(c => new CustomerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone
            });
        }

        // GET BY ID
        public async Task<CustomerResponse?> GetCustomerByIdAsync(int id)
        {
            var customer = await _repo.GetByIdAsync(id);

            if (customer == null)
                return null;

            return new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone
            };
        }

        // UPDATE
        public async Task<bool> UpdateCustomerAsync(int id, CreateCustomerRequest request)
        {
            var customer = await _repo.GetByIdAsync(id);

            if (customer == null)
                return false;

            customer.Name = request.Name;
            customer.Email = request.Email;
            customer.Phone = request.Phone;

            _repo.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // DELETE
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _repo.GetByIdAsync(id);

            if (customer == null)
                return false;

            _repo.Delete(customer);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<dynamic> GetAllAsync()
        {
            var customers = await GetAllCustomersAsync();
            return customers;
        }
    }
}