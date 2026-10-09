using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using HotelBilling.Infrastructure.Repository;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace HotelBillingSolution.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly HotelBillingSolution.Services.IEmailService _emailService;
        private readonly IAppUserRepository _appUserRepo;
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IMemoryCache cache, IConfiguration configuration, HotelBillingSolution.Services.IEmailService emailService, IAppUserRepository appUserRepo, IUserService userService, ILogger<AccountController> logger)
        {
            _cache = cache;
            _configuration = configuration;
            _emailService = emailService;
            _appUserRepo = appUserRepo;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST/GET: /Account/ExternalLogin
        //
        // ✅ FIX: The Sign-In tab's social buttons are plain <a href="..."> links
        // (a GET request), while the Sign-Up tab's social buttons are inside
        // <form method="post"> (a POST request). This action was previously
        // restricted to [HttpPost] only, so the GET request coming from the
        // Sign-In anchors never matched this action at all — ASP.NET Core
        // fell through and the browser ended up back on /Account/Login with
        // the intended URL stuck in the ReturnUrl query string (exactly what
        // you saw in the address bar). Accepting both verbs here fixes both
        // tabs without having to rewrite the markup.
        [HttpGet]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                _logger?.LogWarning("ExternalLogin called with empty provider");
                return RedirectToAction("Login");
            }

            // Only allow known providers
            var supported = new[] { "Google", "Microsoft", "GitHub" };
            if (!supported.Contains(provider))
            {
                _logger?.LogWarning("ExternalLogin called with unsupported provider: {Provider}", provider);
                return RedirectToAction("Login");
            }

            // Ensure the requested authentication scheme is actually registered. If it's not
            // registered we should fail-fast and show the login page rather than letting the
            // authentication middleware throw an exception during Challenge().
            var schemeProvider = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider)) as Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider;
            var scheme = schemeProvider == null ? null : await schemeProvider.GetSchemeAsync(provider);
            if (scheme == null)
            {
                _logger?.LogWarning("ExternalLogin requested for unregistered provider: {Provider}", provider);
                TempData["AuthError"] = "External authentication provider is not configured.";
                return RedirectToAction("Login");
            }

            // Redirect URL after external provider has authenticated
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

            // Properties stored by the external auth middleware
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            // NOTE: This Challenge() call only works if "Google", "Microsoft" and
            // "GitHub" authentication schemes are actually registered in
            // Program.cs (AddAuthentication().AddGoogle(...).AddMicrosoftAccount(...).AddGitHub(...)
            // with real client id/secret). If those handlers aren't registered,
            // clicking the button will throw an InvalidOperationException
            // ("Scheme X not registered") instead of doing anything visible.
            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!string.IsNullOrEmpty(remoteError))
            {
                _logger?.LogWarning("External provider returned an error: {Error}", remoteError);
                TempData["AuthError"] = "External provider error: " + remoteError;
                return RedirectToAction("Login", new { returnUrl });
            }

            // Authenticate the external cookie (scheme 'External') to obtain principal
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded)
            {
                _logger?.LogWarning("External authentication failed: no principal returned");
                return RedirectToAction("Login", new { returnUrl });
            }

            var externalPrincipal = result.Principal;
            var provider = result.Properties?.Items[".AuthScheme"] ?? externalPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Issuer;

            // Extract common claims - email and name
            string email = null;
            try
            {
                email = externalPrincipal?.FindFirst(ClaimTypes.Email)?.Value
                        ?? externalPrincipal?.FindFirst("email")?.Value
                        ?? externalPrincipal?.FindFirst("urn:github:email")?.Value;
            }
            catch { }

            string name = externalPrincipal?.FindFirst(ClaimTypes.Name)?.Value
                          ?? externalPrincipal?.FindFirst("name")?.Value;

            string providerUserId = externalPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? externalPrincipal?.Claims.FirstOrDefault(c => c.Type.EndsWith("id"))?.Value;

            if (string.IsNullOrEmpty(email))
            {
                _logger?.LogWarning("External login did not provide an email claim (provider: {Provider})", provider);
                TempData["AuthError"] = "External provider did not provide an email address.";
                return RedirectToAction("Login", new { returnUrl });
            }

            // Find existing user by email
            AppUser user = null;
            try
            {
                user = await _appUserRepo.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to lookup user by email during external login");
                TempData["AuthError"] = "Authentication failed. Please try again.";
                return RedirectToAction("Login", new { returnUrl });
            }

            if (user == null)
            {
                // Create new user using existing IUserService.RegisterAsync pattern if available
                try
                {
                    // Use username derived from email (before @) if available
                    var username = email.Split('@')[0];
                    var created = await _userService.RegisterAsync(username, email, Guid.NewGuid().ToString("N"));
                    if (!created)
                    {
                        // fallback: try direct repository add
                        var newUser = new AppUser { Username = username, Email = email, CreatedDate = DateTime.UtcNow };
                        await _appUserRepo.AddAsync(newUser);
                        user = newUser;
                    }
                    else
                    {
                        user = await _appUserRepo.GetByEmailAsync(email);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to create local user for external login");
                    TempData["AuthError"] = "Could not create local account for external user.";
                    return RedirectToAction("Login", new { returnUrl });
                }
            }

            // Build application claims from AppUser and sign in using existing cookie scheme
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username ?? user.Email ?? "user"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

            // Remove the external cookie
            await HttpContext.SignOutAsync("External");

            // Redirect safely
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // First try application users from DB
            var userService = HttpContext.RequestServices.GetService(typeof(HotelBilling.Application.Interfaces.IUserService)) as HotelBilling.Application.Interfaces.IUserService;
            var valid = false;

            if (userService != null)
            {
                valid = await userService.ValidateCredentialsAsync(username, password);
            }

            // ✅ FIX: sign in the DB-validated user (this block was missing before)
            if (valid)
            {
                var dbClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "User")
                };

                var dbIdentity = new ClaimsIdentity(dbClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var dbPrincipal = new ClaimsPrincipal(dbIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, dbPrincipal, new AuthenticationProperties
                {
                    IsPersistent = true
                });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Fallback to default admin account
            if (!valid && username == "admin" && password == "123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Administrator")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true
                });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match");
                return View("Login");
            }

            var userService = HttpContext.RequestServices.GetService(typeof(HotelBilling.Application.Interfaces.IUserService)) as HotelBilling.Application.Interfaces.IUserService;
            if (userService == null)
            {
                ModelState.AddModelError(string.Empty, "Registration not available");
                return View("Login");
            }

            var created = await userService.RegisterAsync(username, email, password);
            if (!created)
            {
                ModelState.AddModelError(string.Empty, "User already exists");
                return View("Login");
            }

            // Auto-login after registration
            return await Login(username, password);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ----------------- FORGOT PASSWORD -----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Please enter your email." });
            }

            var userService = HttpContext.RequestServices.GetService(typeof(HotelBilling.Application.Interfaces.IUserService)) as HotelBilling.Application.Interfaces.IUserService;
            if (userService == null)
            {
                return Json(new { success = false, message = "Service unavailable. Please try again later." });
            }

            // Requires: Task<bool> EmailExistsAsync(string email) on IUserService
            var exists = await userService.EmailExistsAsync(email);
            if (!exists)
            {
                return Json(new { success = false, message = "No account found with this email." });
            }

            // Generate secure 6 digit OTP and store for 5 minutes
            var otp = _emailService.GenerateOtp();
            _emailService.StoreOtp(email, otp);

            try
            {
                await _emailService.SendOtpAsync(email, otp);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to send OTP email. Please try again." });
            }

            return Json(new { success = true, message = "OTP sent to your email." });
        }

        // ----------------- RESET PASSWORD -----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            ViewData["Email"] = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword, string confirmNewPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            {
                return Json(new { success = false, message = "Email and OTP are required." });
            }

            if (newPassword != confirmNewPassword)
            {
                return Json(new { success = false, message = "Passwords do not match." });
            }

            if (!_emailService.TryGetOtp(email, out var cachedOtp))
            {
                return Json(new { success = false, message = "OTP expired. Please request a new one." });
            }

            if (cachedOtp != otp)
            {
                return Json(new { success = false, message = "Invalid OTP. Please try again." });
            }

            var userService = HttpContext.RequestServices.GetService(typeof(HotelBilling.Application.Interfaces.IUserService)) as HotelBilling.Application.Interfaces.IUserService;
            if (userService == null)
            {
                return Json(new { success = false, message = "Service unavailable. Please try again later." });
            }

            // Requires: Task<bool> ResetPasswordAsync(string email, string newPassword) on IUserService
            var updated = await userService.ResetPasswordAsync(email, newPassword);
            if (!updated)
            {
                return Json(new { success = false, message = "Could not reset password. Please try again." });
            }

            // OTP used — remove it so it can't be reused
            _emailService.RemoveOtp(email);

            return Json(new { success = true, message = "Password reset successful." });
        }

        // ----------------- EMAIL HELPER -----------------

        private async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            // appsettings.json:
            // "EmailSettings": {
            //   "SenderEmail": "nikitakhune95@gmail.com",
            //   "SenderPassword": "your-16-char-gmail-app-password",
            //   "SmtpHost": "smtp.gmail.com",
            //   "SmtpPort": 587
            // }

            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "nikitakhune95@gmail.com";
            var senderPassword = _configuration["EmailSettings:SenderPassword"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Hotel Billing Management System"),
                Subject = "Your Password Reset OTP",
                Body = $"Hello,\n\nYour OTP to reset your Hotel Billing account password is: {otp}\n\nThis OTP is valid for 10 minutes. If you did not request this, please ignore this email.\n\nRegards,\nHotel Billing Management System",
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}
