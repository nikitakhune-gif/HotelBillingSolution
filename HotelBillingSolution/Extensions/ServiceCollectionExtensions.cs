using Microsoft.Extensions.DependencyInjection;
using HotelBilling.Application.Interfaces;
using HotelBilling.Application.Services;

namespace HotelBilling.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application layer services here
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IBillingService, BillingService>();
            // Register settings service implementation
            services.AddScoped<ISettingsService, SettingsService>();

            // Add more services if needed
            // e.g., services.AddScoped<IReportService, ReportService>();

            return services;
        }
    }
}