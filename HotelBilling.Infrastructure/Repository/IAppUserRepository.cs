using HotelBilling.Domain.Entities;
using System.Threading.Tasks;

namespace HotelBilling.Infrastructure.Repository
{
    public interface IAppUserRepository
    {
        Task<AppUser> GetByIdAsync(string id);
        Task<AppUser> GetByEmailAsync(string email);
        Task<AppUser> GetByUsernameAsync(string username);
        Task AddAsync(AppUser user);
        Task UpdateAsync(AppUser user);
    }
}
