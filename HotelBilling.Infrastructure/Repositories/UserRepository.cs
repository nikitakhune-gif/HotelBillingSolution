using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace HotelBilling.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<AppUser>
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<AppUser?> GetByUsernameAsync(string username)
            => await _dbSet.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<AppUser?> GetByEmailAsync(string email)
            => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}
