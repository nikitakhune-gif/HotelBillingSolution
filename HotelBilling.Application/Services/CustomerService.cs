using HotelBilling.Application.DTOs.Customer;
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
        public async Task<int> CreateCustomerAsync(CustomerDto request)
        {
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                MobileNumber = request.MobileNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Nationality = request.Nationality,
                ProfilePhotoPath = request.ProfilePhotoPath,
                CustomerType = request.CustomerType,
                Membership = request.Membership,
                Status = request.Status,
                Username = request.Username,
                PasswordHash = request.Password ?? string.Empty,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                Country = request.Country,
                PostalCode = request.PostalCode,
                IdProofType = request.IdProofType,
                IdNumber = request.IdNumber,
                CompanyName = request.CompanyName,
                Occupation = request.Occupation,
                GstNumber = request.GstNumber,
                EmergencyContactPerson = request.EmergencyContactPerson,
                EmergencyRelationship = request.EmergencyRelationship,
                EmergencyMobile = request.EmergencyMobile,
                EmergencyAltMobile = request.EmergencyAltMobile,
                CustomerNotes = request.CustomerNotes,
                SpecialRequirements = request.SpecialRequirements,
                PreferredRoomType = request.PreferredRoomType,
                MealPreference = request.MealPreference
            };

            await _repo.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return customer.Id;
        }

        // GET ALL
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repo.GetAllAsync();

            return customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                MobileNumber = c.MobileNumber,
                DateOfBirth = c.DateOfBirth,
                CreatedDate = c.CreatedDate,
                Gender = c.Gender,
                Nationality = c.Nationality,
                ProfilePhotoPath = c.ProfilePhotoPath,
                CustomerType = c.CustomerType,
                Membership = c.Membership,
                Status = c.Status,
                Username = c.Username,
                AddressLine1 = c.AddressLine1,
                AddressLine2 = c.AddressLine2,
                City = c.City,
                State = c.State,
                Country = c.Country,
                PostalCode = c.PostalCode
            });
        }

        // GET BY ID
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _repo.GetByIdAsync(id);

            if (customer == null)
                return null;

            return new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                MobileNumber = customer.MobileNumber,
                DateOfBirth = customer.DateOfBirth,
                CreatedDate = customer.CreatedDate,
                Gender = customer.Gender,
                Nationality = customer.Nationality,
                ProfilePhotoPath = customer.ProfilePhotoPath,
                CustomerType = customer.CustomerType,
                Membership = customer.Membership,
                Status = customer.Status,
                Username = customer.Username,
                AddressLine1 = customer.AddressLine1,
                AddressLine2 = customer.AddressLine2,
                City = customer.City,
                State = customer.State,
                Country = customer.Country,
                PostalCode = customer.PostalCode,
                IdProofType = customer.IdProofType,
                IdNumber = customer.IdNumber,
                CompanyName = customer.CompanyName,
                Occupation = customer.Occupation,
                GstNumber = customer.GstNumber,
                EmergencyContactPerson = customer.EmergencyContactPerson,
                EmergencyRelationship = customer.EmergencyRelationship,
                EmergencyMobile = customer.EmergencyMobile,
                EmergencyAltMobile = customer.EmergencyAltMobile,
                CustomerNotes = customer.CustomerNotes,
                SpecialRequirements = customer.SpecialRequirements,
                PreferredRoomType = customer.PreferredRoomType,
                MealPreference = customer.MealPreference
            };
        }

        // UPDATE
        public async Task<bool> UpdateCustomerAsync(int id, CustomerDto request)
        {
            var customer = await _repo.GetByIdAsync(id);

            if (customer == null)
                return false;

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;
            customer.MobileNumber = request.MobileNumber;
            // Ensure ProfilePhotoPath and address/other updatable fields are persisted
            if (!string.IsNullOrEmpty(request.ProfilePhotoPath)) customer.ProfilePhotoPath = request.ProfilePhotoPath;
            customer.DateOfBirth = request.DateOfBirth;
            customer.Gender = request.Gender;
            customer.Nationality = request.Nationality;
            customer.CustomerType = request.CustomerType;
            customer.Membership = request.Membership;
            customer.Status = request.Status;
            customer.Username = request.Username ?? customer.Username;
            customer.AddressLine1 = request.AddressLine1;
            customer.AddressLine2 = request.AddressLine2;
            customer.City = request.City;
            customer.State = request.State;
            customer.Country = request.Country;
            customer.PostalCode = request.PostalCode;
            customer.IdProofType = request.IdProofType;
            customer.IdNumber = request.IdNumber;
            customer.CompanyName = request.CompanyName;
            customer.Occupation = request.Occupation;
            customer.GstNumber = request.GstNumber;
            customer.EmergencyContactPerson = request.EmergencyContactPerson;
            customer.EmergencyRelationship = request.EmergencyRelationship;
            customer.EmergencyMobile = request.EmergencyMobile;
            customer.EmergencyAltMobile = request.EmergencyAltMobile;
            customer.CustomerNotes = request.CustomerNotes;
            customer.SpecialRequirements = request.SpecialRequirements;
            customer.PreferredRoomType = request.PreferredRoomType;
            customer.MealPreference = request.MealPreference;

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