using HotelBilling.Application.Interfaces;
using HotelBilling.Application.Services;
using HotelBilling.Domain.Interfaces;
using HotelBilling.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using HotelBilling.Infrastructure.Data.DbContext;
using HotelBilling.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

// Added for new authentication module
using HotelBilling.Infrastructure.Repository;
using HotelBilling.Infrastructure.Services;
using HotelBilling.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5051");

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// SignalR for chat hub
builder.Services.AddSignalR();
// Add memory cache for OTP storage
builder.Services.AddMemoryCache();

// Register EmailService
builder.Services.AddScoped<HotelBillingSolution.Services.IEmailService, HotelBillingSolution.Services.EmailService>();

// Authentication: configured below together with external providers

// Require authenticated users by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Register DbContext
// NOTE: AppDbContext is implemented in the Infrastructure project.
// Set the MigrationsAssembly to the Web project's assembly so EF Core
// tools operate against the same migrations location that currently exists.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // Migrations will be kept in the Infrastructure project (recommended for Clean Architecture).
            sqlOptions.MigrationsAssembly("HotelBillingWeb");
            sqlOptions.EnableRetryOnFailure();
        }
    ));

// External authentication providers configuration (Google / Microsoft / GitHub)
// Providers' ClientId/ClientSecret are read from configuration (appsettings.json or user-secrets)
var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "HotelBillingAuth";
    })
    // Temporary cookie where external providers will store the external identity during the callback.
    .AddCookie("External", options =>
    {
        options.Cookie.Name = "ExternalAuth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    });

// Register external providers. Each provider will write the external principal into the
// "External" cookie by setting options.SignInScheme = "External" so the existing
// ExternalLoginCallback in AccountController can process it.
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle("Google", options =>
    {
        options.SignInScheme = "External";
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
    });
}

var msClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
var msClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
if (!string.IsNullOrWhiteSpace(msClientId) && !string.IsNullOrWhiteSpace(msClientSecret))
{
    authBuilder.AddMicrosoftAccount("Microsoft", options =>
    {
        options.SignInScheme = "External";
        options.ClientId = msClientId;
        options.ClientSecret = msClientSecret;
        options.CallbackPath = "/signin-microsoft";
        options.SaveTokens = true;
    });
}

var ghClientId = builder.Configuration["Authentication:GitHub:ClientId"];
var ghClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
if (!string.IsNullOrWhiteSpace(ghClientId) && !string.IsNullOrWhiteSpace(ghClientSecret))
{
    authBuilder.AddGitHub("GitHub", options =>
    {
        options.SignInScheme = "External";
        options.ClientId = ghClientId;
        options.ClientSecret = ghClientSecret;
        options.CallbackPath = "/signin-github";
        options.Scope.Add("user:email");
        options.SaveTokens = true;
        
    });
}


// Register services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
// Settings service
builder.Services.AddScoped<HotelBilling.Application.Interfaces.ISettingsService, HotelBilling.Application.Services.SettingsService>();
// Booking service registration
builder.Services.AddScoped<HotelBilling.Application.Interfaces.IBookingService, HotelBilling.Application.Services.BookingService>();
// Payment service
builder.Services.AddScoped<HotelBilling.Application.Interfaces.IPaymentService, HotelBilling.Application.Services.PaymentService>();
// Housekeeping service
builder.Services.AddScoped<HotelBilling.Application.Interfaces.IHousekeepingService, HotelBilling.Application.Services.HousekeepingService>();

// Dashboard service
builder.Services.AddScoped<HotelBilling.Application.Interfaces.IDashboardService, HotelBilling.Application.Services.DashboardService>();

// User service
builder.Services.AddScoped<HotelBilling.Application.Interfaces.IUserService, HotelBilling.Application.Services.UserService>();
// Chat service
builder.Services.AddScoped<HotelBilling.Application.Interfaces.IChatService, HotelBilling.Application.Services.ChatService>();
// Register AutoMapper using the single consolidated MappingProfile
builder.Services.AddAutoMapper(typeof(HotelBilling.Application.Mappings.MappingProfile).Assembly);
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Auth module registrations
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>(); // uses AppDbContext
builder.Services.AddScoped<IAuthService, AuthService>(); // uses AppDbContext
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
// Ensure Authentication runs before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();