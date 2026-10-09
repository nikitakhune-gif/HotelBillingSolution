using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using HotelBillingSolution.Services;

namespace HotelBillingSolution.Controllers
{
    [AllowAnonymous]
    public class OtpController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public OtpController(IEmailService emailService, IMemoryCache cache)
        {
            _emailService = emailService;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public class EmailDto { public string Email { get; set; } }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] EmailDto dto)
        {
            var email = dto?.Email;
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Email required" });

            // Throttle repeated requests
            if (_emailService.IsThrottled(email))
            {
                return Json(new { success = false, message = "Please wait before requesting a new OTP." });
            }

            var otp = _emailService.GenerateOtp();
            _emailService.StoreOtp(email, otp);

            try
            {
                await _emailService.SendOtpAsync(email, otp);
            }
            catch
            {
                _emailService.RemoveOtp(email);
                return Json(new { success = false, message = "Failed to send OTP email." });
            }

            return Json(new { success = true, message = "OTP sent" });
        }

        [HttpPost]
        public IActionResult Verify(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                return Json(new { success = false, message = "Email and OTP required" });

            if (!_emailService.TryGetOtp(email, out var cachedOtp))
                return Json(new { success = false, message = "OTP expired or not found" });

            if (cachedOtp != otp)
                return Json(new { success = false, message = "Invalid OTP" });

            // Success — remove OTP so it can't be reused
            _emailService.RemoveOtp(email);

            return Json(new { success = true, message = "OTP verified" });
        }
    }
}
