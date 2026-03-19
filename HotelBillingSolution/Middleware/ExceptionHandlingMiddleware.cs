using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HotelBilling.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An unhandled exception occurred!");

                // Return friendly error page
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html";

                await context.Response.WriteAsync($@"
                    <html>
                        <body style='font-family:Segoe UI, sans-serif; text-align:center; padding:50px;'>
                            <h1>Oops! Something went wrong.</h1>
                            <p>Our team has been notified.</p>
                            <p><strong>Error:</strong> {ex.Message}</p>
                        </body>
                    </html>");
            }
        }
    }
}