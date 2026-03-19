using HotelBilling.Application.Interfaces;
using HotelBilling.Application.Services;
using HotelBilling.Domain.Interfaces;
using HotelBilling.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using HotelBilling.Infrastructure.Data.DbContext;
using HotelBilling.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Authentication: cookie-based
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "HotelBillingAuth";
    });

// Require authenticated users by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("HotelBillingWeb");
            sqlOptions.EnableRetryOnFailure();
        }
    ));

// Register services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();