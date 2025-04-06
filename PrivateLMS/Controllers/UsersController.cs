using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace PrivateLMS.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View(new List<string>());
        }
    }
}