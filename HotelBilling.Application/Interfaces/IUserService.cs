using HotelBilling.Application.DTOs;
using System.Threading.Tasks;

namespace HotelBilling.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string username, string email, string password);

        Task<bool> ValidateCredentialsAsync(string username, string password);

        Task<bool> EmailExistsAsync(string email);

        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
}
