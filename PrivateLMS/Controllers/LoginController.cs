using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using PrivateLMS.Data;
using Microsoft.AspNetCore.Http;

namespace PrivateLMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly LibraryDbContext _context;

        public LoginController(LibraryDbContext context)
        {
            _context = context;
        }

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
                // Check credentials against the database
                var user = _context.Users
                    .FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("Username", model.Username);
                    HttpContext.Session.SetString("Role", user.Role); 
                    return RedirectToAction("Index", "Books");
                }

                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            return View(model);
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Books");
        }
    }
}