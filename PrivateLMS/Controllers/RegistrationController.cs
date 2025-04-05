using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using PrivateLMS.Data;

namespace PrivateLMS.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly LibraryDbContext _context;

        public RegistrationController(LibraryDbContext context)
        {
            _context = context;
        }

        // Step 1: Personal Information
        public IActionResult Step1()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Step1(User user)
        {
            if (ModelState.IsValid)
            {
                TempData["FirstName"] = user.FirstName;
                TempData["LastName"] = user.LastName;
                TempData["Gender"] = user.Gender;
                return RedirectToAction("Step2");
            }
            return View(user);
        }

        // Step 2: Account Information
        public IActionResult Step2()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Step2(User user)
        {
            if (ModelState.IsValid)
            {
                TempData["Username"] = user.Username;
                TempData["Password"] = user.Password;
                return RedirectToAction("Step3");
            }
            return View(user);
        }

        // Step 3: Contact Information
        public IActionResult Step3()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Step3(User user)
        {
            if (ModelState.IsValid)
            {
                TempData["PhoneNumber"] = user.PhoneNumber;
                TempData["Email"] = user.Email;
                TempData["Address"] = user.Address;
                TempData["City"] = user.City;
                TempData["State"] = user.State;
                TempData["PostalCode"] = user.PostalCode;
                TempData["Country"] = user.Country;
                return RedirectToAction("Step4");
            }
            return View(user);
        }

        // Step 4: Confirmation
        public IActionResult Step4()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Step4(User user)
        {
            if (user.TermsAccepted)
            {
                user.FirstName = TempData["FirstName"].ToString();
                user.LastName = TempData["LastName"].ToString();
                user.Gender = TempData["Gender"].ToString();
                user.Username = TempData["Username"].ToString();
                user.Password = TempData["Password"].ToString();
                user.PhoneNumber = TempData["PhoneNumber"].ToString();
                user.Email = TempData["Email"].ToString();
                user.Address = TempData["Address"].ToString();
                user.City = TempData["City"].ToString();
                user.State = TempData["State"].ToString();
                user.PostalCode = TempData["PostalCode"].ToString();
                user.Country = TempData["Country"].ToString();
                user.Role = "User";

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Success");
            }
            ModelState.AddModelError("TermsAccepted", "You must accept the terms.");
            return View(user);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
