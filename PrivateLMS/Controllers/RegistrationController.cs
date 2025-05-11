using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public RegistrationController(LibraryDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        public IActionResult Step1()
        {
            Console.WriteLine($"Step1 GET: TempData[FirstName]={TempData["FirstName"]}, TempData[LastName]={TempData["LastName"]}");
            var model = new Step1ViewModel
            {
                FirstName = TempData["FirstName"]?.ToString() ?? string.Empty,
                LastName = TempData["LastName"]?.ToString() ?? string.Empty,
                Gender = TempData["Gender"]?.ToString() ?? string.Empty,
                DateOfBirth = TempData["DateOfBirth"] != null ? DateTime.Parse(TempData["DateOfBirth"].ToString()) : (DateTime?)null

            };
            TempData.Keep();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step1(Step1ViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.DateOfBirth.HasValue)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of birth is required.");
                    return View(model);
                }

                var today = DateTime.Today;
                var age = today.Year - model.DateOfBirth.Value.Year;
                if (model.DateOfBirth.Value > today.AddYears(-age)) age--;

                if (age < 13)
                {
                    ModelState.AddModelError("DateOfBirth", "You must be at least 13 years old to register.");
                    return View(model);
                }

                try
                {
                    TempData["FirstName"] = model.FirstName;
                    TempData["LastName"] = model.LastName;
                    TempData["Gender"] = model.Gender;
                    TempData["DateOfBirth"] = model.DateOfBirth?.ToString("yyyy-MM-dd");
                    TempData.Keep();
                    return RedirectToAction("Step2");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult Step2()
        {
            var model = new Step2ViewModel
            {
                Username = TempData["Username"]?.ToString() ?? string.Empty,
                Email = TempData["Email"]?.ToString() ?? string.Empty,
                Password = TempData["Password"]?.ToString() ?? string.Empty,
                ConfirmPassword = TempData["ConfirmPassword"]?.ToString() ?? string.Empty
            };
            TempData.Keep();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step2(Step2ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate Username
                var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View(model);
                }

                // Validate Email
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // Validate Password with all validators
                var passwordErrors = new List<string>();
                foreach (var validator in _userManager.PasswordValidators)
                {
                    var passwordResult = await validator.ValidateAsync(_userManager, null, model.Password);
                    if (!passwordResult.Succeeded)
                    {
                        passwordErrors.AddRange(passwordResult.Errors.Select(e => e.Description));
                    }
                }

                if (passwordErrors.Any())
                {
                    foreach (var error in passwordErrors)
                    {
                        ModelState.AddModelError("Password", error);
                    }
                    return View(model);
                }

                // Store validated data in TempData
                TempData["Username"] = model.Username;
                TempData["Password"] = model.Password;
                TempData["ConfirmPassword"] = model.ConfirmPassword;
                TempData["Email"] = model.Email;
                TempData.Keep();
                return RedirectToAction("Step3");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        public IActionResult Step3()
        {
            var model = new Step3ViewModel
            {
                PhoneNumber = TempData["PhoneNumber"]?.ToString() ?? string.Empty,
                Address = TempData["Address"]?.ToString() ?? string.Empty,
                City = TempData["City"]?.ToString() ?? string.Empty,
                State = TempData["State"]?.ToString() ?? string.Empty,
                PostalCode = TempData["PostalCode"]?.ToString() ?? string.Empty,
                Country = TempData["Country"]?.ToString() ?? string.Empty,
                // Populate dropdown lists
                Countries = new List<string> { "Nigeria" },
                States = new List<string> { "Abia", "Adamawa", "Akwa Ibom", "Anambra", "Bauchi", "Bayelsa", "Benue", "Borno", "Cross River", "Delta", "Ebonyi", "Edo", "Ekiti", "Enugu", "Gombe", "Imo", "Jigawa", "Kaduna", "Kano", "Katsina", "Kebbi", "Kogi", "Kwara", "Lagos", "Nasarawa", "Niger", "Ogun", "Ondo", "Osun", "Oyo", "Plateau", "Rivers", "Sokoto", "Taraba", "Yobe", "Zamfara", "FCT" } // Nigerian states
            };
            TempData.Keep();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step3(Step3ViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["PhoneNumber"] = model.PhoneNumber;
                TempData["Address"] = model.Address;
                TempData["City"] = model.City;
                TempData["State"] = model.State;
                TempData["PostalCode"] = model.PostalCode;
                TempData["Country"] = model.Country;
                TempData.Keep();
                return RedirectToAction("Step4");
            }
            return View(model);
        }

        public IActionResult Step4()
        {
            TempData.Keep();
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
                    // Validate required TempData values
                    if (TempData["Username"] == null || TempData["Email"] == null || TempData["Password"] == null ||
                        TempData["FirstName"] == null || TempData["LastName"] == null || TempData["Gender"] == null ||
                        TempData["DateOfBirth"] == null || TempData["PhoneNumber"] == null || TempData["Address"] == null)
                    {
                        ModelState.AddModelError("", "Required registration data is missing. Please start over.");
                        return View(model);
                    }

                    var user = new ApplicationUser
                    {
                        UserName = TempData["Username"].ToString(),
                        Email = TempData["Email"].ToString(),
                        PhoneNumber = TempData["PhoneNumber"].ToString(),
                        FirstName = TempData["FirstName"].ToString(),
                        LastName = TempData["LastName"].ToString(),
                        Gender = TempData["Gender"].ToString(),
                        DateOfBirth = DateTime.Parse(TempData["DateOfBirth"].ToString()),
                        Address = TempData["Address"].ToString(),
                        City = TempData["City"]?.ToString(),
                        State = TempData["State"]?.ToString(),
                        PostalCode = TempData["PostalCode"]?.ToString(),
                        Country = TempData["Country"]?.ToString(),
                        TermsAccepted = true,
                        IsApproved = false,
                        EmailConfirmed = false
                    };

                    var password = TempData["Password"].ToString();
                    var confirmPassword = TempData["ConfirmPassword"].ToString();

                    if (password != confirmPassword)
                    {
                        ModelState.AddModelError("", "Passwords do not match.");
                        return View(model);
                    }

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");

                        // Generate email verification token for user
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Registration", new { userId = user.Id, token }, protocol: Request.Scheme);

                        // Send verification email to user
                        var userEmailBody = $"Please confirm your email by clicking <a href='{callbackUrl}'>here</a>.";
                        await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", userEmailBody);

                        // Send notification email to admin
                        var adminEmailBody = $@"
                    <h2>New User Registration</h2>
                    <p>A new user has registered with the following details:</p>
                    <ul>
                        <li><strong>Username:</strong> {user.UserName}</li>
                        <li><strong>Email:</strong> {user.Email}</li>
                        <li><strong>Full Name:</strong> {user.FirstName} {user.LastName}</li>
                        <li><strong>Registration Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy HH:mm}</li>
                    </ul>
                    <p>Please review the user in the admin panel and approve their account if necessary.</p>
                    <p>Best regards,<br/>Warathatul Ambiya Library System</p>";
                        await _emailService.SendEmailAsync("admin@warathatulambiya.com", "New User Registration", adminEmailBody);

                        // Clear TempData after successful registration
                        TempData.Clear();

                        TempData["SuccessMessage"] = "Registration successful! Please check your email to verify your account.";
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

            TempData.Keep(); // Preserve TempData if validation fails
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            if (userId == 0 || string.IsNullOrEmpty(token))
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }

            return View("Error");
        }

        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendVerificationEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
            {
                TempData["ErrorMessage"] = "Invalid email or email already verified.";
                return View();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Registration", new { userId = user.Id, token }, protocol: Request.Scheme);
            var emailBody = $"Please confirm your email by clicking <a href='{callbackUrl}'>here</a>.";
            await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", emailBody);

            TempData["SuccessMessage"] = "Verification email resent. Please check your inbox.";
            return RedirectToAction("Success");
        }
    }
}