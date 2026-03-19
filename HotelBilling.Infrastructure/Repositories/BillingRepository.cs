using HotelBilling.Domain.Entities;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace HotelBilling.Infrastructure.Repositories
{
    public class BillingRepository : GenericRepository<Bill>
    {
        public BillingRepository(AppDbContext context) : base(context)
        {
        }

        // Custom Method Example
        public async Task<IEnumerable<Bill>> GetBillsWithDetailsAsync()
        {
            return await _context.Bills
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .ToListAsync();
        }
    }
}