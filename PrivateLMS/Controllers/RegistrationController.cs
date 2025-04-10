using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [AllowAnonymous]
    public class RegistrationController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationController(LibraryDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Step1()
        {
            return View(new Step1ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public IActionResult Step2()
        {
            return View(new Step2ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public IActionResult Step3()
        {
            return View(new Step3ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public IActionResult Step4()
        {
            return View(new Step4ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4(Step4ViewModel model)
        {
            if (!model.TermsAccepted)
            {
                ModelState.AddModelError("TermsAccepted", "You must accept the terms.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = TempData["Username"]?.ToString() ?? throw new Exception("Username is missing."),
                        Email = TempData["Email"]?.ToString(),
                        PhoneNumber = TempData["PhoneNumber"]?.ToString(),
                        FirstName = TempData["FirstName"]?.ToString() ?? throw new Exception("FirstName is missing."),
                        LastName = TempData["LastName"]?.ToString() ?? throw new Exception("LastName is missing."),
                        Gender = TempData["Gender"]?.ToString() ?? throw new Exception("Gender is missing."),
                        DateOfBirth = DateTime.Parse(TempData["DateOfBirth"]?.ToString() ?? throw new Exception("DateOfBirth is missing.")),
                        Address = TempData["Address"]?.ToString() ?? throw new Exception("Address is missing."),
                        City = TempData["City"]?.ToString(),
                        State = TempData["State"]?.ToString(),
                        PostalCode = TempData["PostalCode"]?.ToString(),
                        Country = TempData["Country"]?.ToString(),
                        TermsAccepted = true
                    };

                    var password = TempData["Password"]?.ToString() ?? throw new Exception("Password is missing.");
                    var confirmPassword = TempData["ConfirmPassword"]?.ToString() ?? throw new Exception("ConfirmPassword is missing.");

                    if (password != confirmPassword)
                    {
                        ModelState.AddModelError("", "Passwords do not match.");
                        return View(model);
                    }

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        TempData["SuccessMessage"] = "Registration successful! Please log in.";
                        return RedirectToAction("Success");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                }
            }

            return View(model);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}