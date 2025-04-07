using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using PrivateLMS.Data;
using PrivateLMS.ViewModels;

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
            return View(new Step1ViewModel());
        }

        [HttpPost]
        public IActionResult Step1(Step1ViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["FirstName"] = model.FirstName;
                TempData["LastName"] = model.LastName;
                TempData["Gender"] = model.Gender;
                TempData["DateOfBirth"] = model.DateOfBirth.ToString("yyyy-MM-dd");
                return RedirectToAction("Step2");
            }
            return View(model);
        }

        // Step 2: Account Information
        public IActionResult Step2()
        {
            return View(new Step2ViewModel());
        }

        [HttpPost]
        public IActionResult Step2(Step2ViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["Username"] = model.Username;
                TempData["Password"] = model.Password;
                TempData["ConfirmPassword"] = model.ConfirmPassword;
                return RedirectToAction("Step3");
            }
            return View(model);
        }

        // Step 3: Contact Information
        public IActionResult Step3()
        {
            return View(new Step3ViewModel());
        }

        [HttpPost]
        public IActionResult Step3(Step3ViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["PhoneNumber"] = model.PhoneNumber;
                TempData["Email"] = model.Email;
                TempData["Address"] = model.Address;
                TempData["City"] = model.City;
                TempData["State"] = model.State;
                TempData["PostalCode"] = model.PostalCode;
                TempData["Country"] = model.Country;
                return RedirectToAction("Step4");
            }
            return View(model);
        }

        // Step 4: Confirmation
        public IActionResult Step4()
        {
            return View(new Step4ViewModel());
        }

        [HttpPost]
        public IActionResult Step4(Step4ViewModel model)
        {
            if (model.TermsAccepted)
            {
                try
                {
                    var user = new User
                    {
                        FirstName = TempData["FirstName"]?.ToString() ?? throw new Exception("FirstName is missing."),
                        LastName = TempData["LastName"]?.ToString() ?? throw new Exception("LastName is missing."),
                        Gender = TempData["Gender"]?.ToString() ?? throw new Exception("Gender is missing."),
                        DateOfBirth = DateTime.Parse(TempData["DateOfBirth"]?.ToString() ?? throw new Exception("DateOfBirth is missing.")),
                        Username = TempData["Username"]?.ToString() ?? throw new Exception("Username is missing."),
                        Password = TempData["Password"]?.ToString() ?? throw new Exception("Password is missing."),
                        ConfirmPassword = TempData["ConfirmPassword"]?.ToString() ?? throw new Exception("ConfirmPassword is missing."),
                        PhoneNumber = TempData["PhoneNumber"]?.ToString(),
                        Email = TempData["Email"]?.ToString(),
                        Address = TempData["Address"]?.ToString(),
                        City = TempData["City"]?.ToString(),
                        State = TempData["State"]?.ToString(),
                        PostalCode = TempData["PostalCode"]?.ToString(),
                        Country = TempData["Country"]?.ToString(),
                        Role = "User",
                        TermsAccepted = true
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Success");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                }
            }
            ModelState.AddModelError("TermsAccepted", "You must accept the terms.");
            return View(model);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}