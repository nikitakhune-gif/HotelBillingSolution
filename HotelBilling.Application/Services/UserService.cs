using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly IUnitOfWork _uow;

        public UserService(IRepository<AppUser> userRepo, IUnitOfWork uow)
        {
            _userRepo = userRepo;
            _uow = uow;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            // check exists
            var exists = (await _userRepo.FindAsync(u => u.Username == username || u.Email == email)).Any();
            if (exists) return false;

            var user = new AppUser
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedDate = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var users = await _userRepo.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();
            if (user == null) return false;
            return VerifyPassword(password, user.PasswordHash);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            var users = await _userRepo.FindAsync(u => u.Email == email);
            return users.Any();
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var users = await _userRepo.FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();

            if (user == null)
                return false;

            user.PasswordHash = HashPassword(newPassword);

            _userRepo.Update(user);
            await _uow.SaveChangesAsync();

            return true;
        }
        private static bool VerifyPassword(string password, string hash)
        {
            var hashed = HashPassword(password);
            return hashed == hash;
        }
    }
}
