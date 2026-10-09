using System.Threading.Tasks;

namespace HotelBillingSolution.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOtpAsync(string toEmail, string otp);
        string GenerateOtp();
        // OTP storage and management
        void StoreOtp(string email, string otp);
        bool TryGetOtp(string email, out string otp);
        void RemoveOtp(string email);
        bool IsThrottled(string email);
    }
}
