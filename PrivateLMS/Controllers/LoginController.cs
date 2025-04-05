using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using Microsoft.AspNetCore.Http;

namespace PrivateLMS.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Login model)
        {
            if (ModelState.IsValid)
            {
                // Simulated user verification (Replace with actual database check)
                if (model.Username == "admin" && model.Password == "password")
                {
                    HttpContext.Session.SetString("Username", model.Username);
                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            return View(model);
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
