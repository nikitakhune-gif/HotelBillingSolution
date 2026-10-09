using HotelBillingSolution.Domain.Entities;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBilling.Infrastructure.Repositories
{
    public class HousekeepingRepository : GenericRepository<HousekeepingTask>
    {
        public HousekeepingRepository(AppDbContext context) : base(context)
        {
        }

        // Custom Method Example
        public async Task<IEnumerable<HousekeepingTask>> GetTasksByRoomAsync(int roomId)
        {
            return await _context.Set<HousekeepingTask>()
                .Where(t => t.RoomId == roomId && !t.IsDeleted)
                .ToListAsync();
        }
    }
}
