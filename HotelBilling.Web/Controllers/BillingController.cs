using Microsoft.AspNetCore.Mvc;

namespace HotelBilling.Web.Controllers
{
    public class BillingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
