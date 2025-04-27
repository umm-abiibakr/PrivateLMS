using Microsoft.AspNetCore.Mvc;

namespace PrivateLMS.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
