using Microsoft.AspNetCore.Mvc;

namespace PrivateLMS.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
