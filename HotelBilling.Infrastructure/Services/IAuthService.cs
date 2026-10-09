using HotelBilling.Application.DTOs;
using HotelBilling.Domain.Entities;
using System.Threading.Tasks;

namespace HotelBilling.Infrastructure.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Error)> RegisterAsync(RegisterDto dto);
        Task<(bool Success, AppUser User, string Error)> ValidateUserAsync(LoginDto dto);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<(bool Success, string Error)> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
