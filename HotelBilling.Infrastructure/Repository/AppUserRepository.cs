using HotelBilling.Domain.Entities;
using HotelBilling.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HotelBilling.Infrastructure.Repository
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly AppDbContext _db;

        public AppUserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(AppUser user)
        {
            await _db.AppUsers.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task<AppUser> GetByEmailAsync(string email)
        {
            return await _db.AppUsers.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            return await _db.AppUsers.FindAsync(id);
        }

        public async Task<AppUser> GetByUsernameAsync(string username)
        {
            return await _db.AppUsers.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task UpdateAsync(AppUser user)
        {
            _db.AppUsers.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
