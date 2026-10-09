using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HotelBillingSolution.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        // OTP settings
        private const int OtpLength = 6;
        private readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);
        private readonly TimeSpan OtpRequestThrottle = TimeSpan.FromMinutes(1);

        public EmailService(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var sender = _configuration["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("Sender email not configured");
            var host = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var password = _configuration["EmailSettings:Password"] ?? throw new InvalidOperationException("Email password not configured. Use user-secrets or env var.");

            var mail = new MailMessage
            {
                From = new MailAddress(sender, "HotelBilling"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials = new NetworkCredential(sender, password);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }

        public Task SendOtpAsync(string toEmail, string otp)
        {
            var subject = "Your HotelBilling OTP";
            var body = $"Your OTP is: {otp}\nThis code will expire in {OtpTtl.TotalMinutes} minutes.";
            return SendEmailAsync(toEmail, subject, body);
        }

        public string GenerateOtp()
        {
            // Generate a cryptographically secure 6-digit number
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var value = BitConverter.ToUInt32(bytes, 0) % 1000000;
                return value.ToString("D6");
            }
        }

        // OTP storage helpers (using IMemoryCache)
        public void StoreOtp(string email, string otp)
        {
            var key = GetOtpCacheKey(email);
            _cache.Set(key, otp, OtpTtl);
            // store throttle timestamp
            _cache.Set(GetThrottleCacheKey(email), DateTime.UtcNow, OtpRequestThrottle);
        }

        public bool TryGetOtp(string email, out string otp)
        {
            return _cache.TryGetValue(GetOtpCacheKey(email), out otp);
        }

        public void RemoveOtp(string email)
        {
            _cache.Remove(GetOtpCacheKey(email));
            _cache.Remove(GetThrottleCacheKey(email));
        }

        public bool IsThrottled(string email)
        {
            return _cache.TryGetValue(GetThrottleCacheKey(email), out DateTime _);
        }

        private string GetOtpCacheKey(string email) => $"otp_{email.ToLowerInvariant()}";
        private string GetThrottleCacheKey(string email) => $"otp_throttle_{email.ToLowerInvariant()}";
    }
}
