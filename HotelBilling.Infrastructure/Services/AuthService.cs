using HotelBilling.Application.DTOs;
using HotelBilling.Domain.Entities;
using HotelBilling.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppUserRepository _repo;
        private readonly IPasswordHasher<AppUser> _hasher;

        public AuthService(IAppUserRepository repo, IPasswordHasher<AppUser> hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<(bool Success, string Error)> RegisterAsync(RegisterDto dto)
        {
            var existingByEmail = await _repo.GetByEmailAsync(dto.Email);
            if (existingByEmail != null)
                return (false, "Email already in use");

            var existingByUsername = await _repo.GetByUsernameAsync(dto.Username);
            if (existingByUsername != null)
                return (false, "Username already in use");

            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            await _repo.AddAsync(user);
            return (true, null);
        }

        public async Task<(bool Success, AppUser User, string Error)> ValidateUserAsync(LoginDto dto)
        {
            AppUser user = null;
            if (dto.Identifier.Contains("@"))
            {
                user = await _repo.GetByEmailAsync(dto.Identifier);
            }
            else
            {
                user = await _repo.GetByUsernameAsync(dto.Identifier);
            }

            if (user == null)
                return (false, null, "Invalid credentials");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Success)
                return (true, user, null);

            return (false, null, "Invalid credentials");
        }

        public Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            // Simple token generation for demonstration. In production use a more secure mechanism.
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{Guid.NewGuid()}"));
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            return Task.FromResult(encoded);
        }

        public async Task<(bool Success, string Error)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _repo.GetByIdAsync(dto.UserId);
            if (user == null)
                return (false, "Invalid user");

            // Decode and validate token (for demo we just check non-empty)
            if (string.IsNullOrEmpty(dto.Token))
                return (false, "Invalid token");

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
            await _repo.UpdateAsync(user);
            return (true, null);
        }
    }
}
