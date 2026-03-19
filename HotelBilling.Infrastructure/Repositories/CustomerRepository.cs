using HotelBilling.Domain.Entities;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace HotelBilling.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>
    {
        public CustomerRepository(AppDbContext context) : base(context)
        {
        }

        // Custom Method Example
        public async Task<Customer?> GetCustomerWithBillsAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Bills)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}