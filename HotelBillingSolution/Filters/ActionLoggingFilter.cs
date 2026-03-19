using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace HotelBilling.Web.Filters
{
    public class ActionLoggingFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Runs before the action executes
            var controller = context.Controller.ToString();
            var action = context.ActionDescriptor.DisplayName;
            var timestamp = DateTime.Now;

            Console.WriteLine($"[LOG] {timestamp}: Executing {controller} → {action}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Runs after the action executes
            var controller = context.Controller.ToString();
            var action = context.ActionDescriptor.DisplayName;
            var timestamp = DateTime.Now;

            Console.WriteLine($"[LOG] {timestamp}: Executed {controller} → {action}");
        }
    }
}