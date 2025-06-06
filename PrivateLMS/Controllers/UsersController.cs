﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;
        private readonly LibraryDbContext _context;
        private readonly ILocationService _locationService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IEmailService emailService,
            LibraryDbContext context,
            ILocationService locationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
            _context = context;
            _locationService = locationService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _userManager.Users
                    .Select(u => new
                    {
                        User = u,
                        ActiveLoanCount = _context.LoanRecords
                            .Count(lr => lr.UserId == u.Id && lr.ReturnDate == null),
                        UnpaidFineCount = _context.Fines
                            .Count(f => f.UserId == u.Id && !f.IsPaid)
                    });

                var totalItems = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.User.UserName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userViewModels = users.Select(u => new UserViewModel
                {
                    Id = u.User.Id,
                    UserName = u.User.UserName,
                    Email = u.User.Email,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName,
                    IsLockedOut = u.User.LockoutEnd.HasValue && u.User.LockoutEnd > DateTimeOffset.UtcNow,
                    IsApproved = u.User.IsApproved,
                    ActiveLoanCount = u.ActiveLoanCount,
                    UnpaidFineCount = u.UnpaidFineCount
                }).ToList();

                var pagedResult = new PagedResultViewModel<UserViewModel>
                {
                    Items = userViewModels,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                };

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading users: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return PartialView("_NotFound");
                }

                var viewModel = new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    TermsAccepted = user.TermsAccepted,
                    IsApproved = user.IsApproved,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading user details: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new UserViewModel();
            ViewBag.Countries = _locationService.GetCountries();
            ViewBag.States = _locationService.GetNigerianStates();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        Gender = model.Gender,
                        DateOfBirth = model.DateOfBirth,
                        Address = model.Address,
                        City = model.City,
                        State = model.State,
                        PostalCode = model.PostalCode,
                        Country = model.Country,
                        TermsAccepted = model.TermsAccepted,
                        IsApproved = model.IsApproved,
                        LockoutEnabled = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        foreach (var role in model.Roles ?? new List<string>())
                        {
                            if (!await _roleManager.RoleExistsAsync(role))
                            {
                                await _roleManager.CreateAsync(new IdentityRole<int> { Name = role, NormalizedName = role.ToUpper() });
                            }
                            await _userManager.AddToRoleAsync(user, role);
                        }

                        TempData["SuccessMessage"] = $"User {user.UserName} created successfully.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the user: {ex.Message}";
                }
            }

            // Repopulate dropdowns on validation failure
            ViewBag.Countries = _locationService.GetCountries();
            ViewBag.States = _locationService.GetNigerianStates();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return PartialView("_NotFound");
                }

                var viewModel = new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    TermsAccepted = user.TermsAccepted,
                    IsApproved = user.IsApproved,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                };

                ViewBag.Countries = _locationService.GetCountries();
                ViewBag.States = _locationService.GetNigerianStates();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading user for editing: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "User ID mismatch.";
                return PartialView("_NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return PartialView("_NotFound");
                    }

                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Gender = model.Gender;
                    user.DateOfBirth = model.DateOfBirth;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.State = model.State;
                    user.PostalCode = model.PostalCode;
                    user.Country = model.Country;
                    user.TermsAccepted = model.TermsAccepted;
                    user.IsApproved = model.IsApproved;
                    user.LockoutEnabled = true;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        ViewBag.Countries = _locationService.GetCountries();
                        ViewBag.States = _locationService.GetNigerianStates();
                        return View(model);
                    }

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(model.Roles ?? new List<string>()).ToList();
                    var rolesToAdd = (model.Roles ?? new List<string>()).Except(currentRoles).ToList();

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }
                    if (rolesToAdd.Any())
                    {
                        foreach (var role in rolesToAdd)
                        {
                            if (!await _roleManager.RoleExistsAsync(role))
                            {
                                await _roleManager.CreateAsync(new IdentityRole<int> { Name = role, NormalizedName = role.ToUpper() });
                            }
                        }
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    TempData["SuccessMessage"] = $"Successfully updated user: {user.UserName}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the user: {ex.Message}";
                }
            }

            ViewBag.Countries = _locationService.GetCountries();
            ViewBag.States = _locationService.GetNigerianStates();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return PartialView("_NotFound");
                }

                var viewModel = new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading user for deletion: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return PartialView("_NotFound");
                }

                // Check only for active loans or unpaid fines
                var hasActiveLoans = await _context.LoanRecords
                    .AnyAsync(lr => lr.UserId == id && lr.ReturnDate == null);

                var hasUnpaidFines = await _context.Fines
                    .AnyAsync(f => f.UserId == id && !f.IsPaid);

                if (hasActiveLoans || hasUnpaidFines)
                {
                    TempData["ErrorMessage"] = "Cannot delete user with active loans or unpaid fines.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete related data 

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = $"User {user.UserName} deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the user: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(int id, string banReason, string duration = "Permanent")
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return PartialView("_NotFound");
                }

                if (string.IsNullOrWhiteSpace(banReason))
                {
                    TempData["ErrorMessage"] = "Ban reason is required.";
                    return RedirectToAction(nameof(Index));
                }

                DateTimeOffset? lockoutEnd;
                switch (duration.ToLower())
                {
                    case "1day":
                        lockoutEnd = DateTimeOffset.UtcNow.AddDays(1);
                        break;
                    case "1week":
                        lockoutEnd = DateTimeOffset.UtcNow.AddDays(7);
                        break;
                    case "1month":
                        lockoutEnd = DateTimeOffset.UtcNow.AddMonths(1);
                        break;
                    case "permanent":
                    default:
                        lockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                        break;
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = $"Failed to ban user: {errors}";
                    return RedirectToAction(nameof(Index));
                }

                // Log ban with reason
                var context = HttpContext.RequestServices.GetService<LibraryDbContext>();
                if (context != null)
                {
                    context.UserActivities.Add(new UserActivity
                    {
                        UserId = user.Id,
                        Action = "Ban",
                        Timestamp = DateTime.UtcNow,
                        Details = $"User banned for {duration} with reason: {banReason}"
                    });
                    await context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"User {user.UserName} has been banned.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while banning the user: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return PartialView("_NotFound");
                }

                await _userManager.SetLockoutEndDateAsync(user, null);

                // Log unban activity
                var context = HttpContext.RequestServices.GetService<LibraryDbContext>();
                if (context != null)
                {
                    context.UserActivities.Add(new UserActivity
                    {
                        UserId = user.Id,
                        Action = "Unban",
                        Timestamp = DateTime.UtcNow,
                        Details = "User unbanned by admin"
                    });
                    await context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"User {user.UserName} has been unbanned.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while unbanning the user: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            // Send approval notification
            var emailBody = $"Your account has been approved. You can now log in at <a href='" +
                $"{Url.Action("Index", "Login", null, Request.Scheme)}'>Warathatul Ambiya</a>.";
            await _emailService.SendEmailAsync(user.Email, "Account Approved", emailBody);

            TempData["SuccessMessage"] = $"{user.UserName} approved.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disapprove(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.IsApproved = false;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"{user.UserName} disapproved.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your profile.";
                    return RedirectToAction("Index", "Login");
                }

                var viewModel = new UserProfileViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    Country = user.Country,
                    ProfilePicturePath = user.ProfilePicturePath
                };

                ViewBag.Countries = _locationService.GetCountries();
                ViewBag.States = _locationService.GetNigerianStates();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your profile: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "You must be logged in to update your profile.";
                        return RedirectToAction("Index", "Login");
                    }

                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.State = model.State;
                    user.PostalCode = model.PostalCode;
                    user.Country = model.Country;

                    if (model.ProfilePicture != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile-pictures");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ProfilePicture.CopyToAsync(stream);
                        }
                        var newProfilePicturePath = $"/images/profile-pictures/{fileName}";

                        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePicturePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        user.ProfilePicturePath = newProfilePicturePath;
                    }

                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        ViewBag.Countries = _locationService.GetCountries();
                        ViewBag.States = _locationService.GetNigerianStates();
                        return View(model);
                    }

                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating your profile: {ex.Message}";
                }
            }

            ViewBag.Countries = _locationService.GetCountries();
            ViewBag.States = _locationService.GetNigerianStates();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendProfileUpdateRequest(
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        string? Email,
        string? Address,
        string? City,
        string? State,
        string? PostalCode,
        string? Country,
        string Reason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to send a profile update request.";
                return RedirectToAction("Profile");
            }

            string adminEmail = "admin@warathatulambiya.com"; 

            string body = $@"
            <h4>Profile Update Request from {user.UserName} ({user.Email})</h4>
            <p><strong>Reason:</strong> {Reason}</p>
            <hr />
            <p><strong>Requested Updates:</strong></p>
            <ul>
                <li><b>First Name:</b> {FirstName}</li>
                <li><b>Last Name:</b> {LastName}</li>
                <li><b>Phone Number:</b> {PhoneNumber}</li>
                <li><b>Email:</b> {Email}</li>
                <li><b>Address:</b> {Address}</li>
                <li><b>City:</b> {City}</li>
                <li><b>State:</b> {State}</li>
                <li><b>Postal Code:</b> {PostalCode}</li>
                <li><b>Country:</b> {Country}</li>
            </ul>
            <p>Sent at: {DateTime.UtcNow}</p>";

            try
            {
                await _emailService.SendEmailAsync(adminEmail, "Profile Update Request", body);
                TempData["SuccessMessage"] = "Your request has been sent to the admin.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to send email: " + ex.Message;
            }

            return RedirectToAction("Profile");
        }

    }
}