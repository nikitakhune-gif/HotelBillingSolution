using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelBilling.Web.Filters
{
    public class AuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing after action
        }
    }
}