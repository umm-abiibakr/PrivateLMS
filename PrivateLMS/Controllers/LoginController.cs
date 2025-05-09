﻿using Microsoft.AspNetCore.Authorization;
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
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "Please verify your email before logging in.");
                        return View(model);
                    }
                    if (!user.IsApproved)
                    {
                        ModelState.AddModelError("", "This account is pending approval. You will be notified once access is granted.");
                        return View(model);
                    }
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "This account is currently banned.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        // Log login
                        var context = HttpContext.RequestServices.GetService<LibraryDbContext>();
                        if (context != null)
                        {
                            context.UserActivities.Add(new UserActivity
                            {
                                UserId = user.Id,
                                Action = "Login",
                                Timestamp = DateTime.UtcNow,
                                Details = $"User logged in at {DateTime.UtcNow}"
                            });
                            await context.SaveChangesAsync();
                        }

                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError("", "This account is currently banned.");
                        return View(model);
                    }
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent.";
                    return RedirectToAction("Index");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Login", new { userId = user.Id, token }, protocol: Request.Scheme);
                var emailBody = $"Please reset your password by clicking <a href='{callbackUrl}'>here</a>.";
                await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

                TempData["SuccessMessage"] = "A password reset link has been sent to your email.";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult ResetPassword()
        {
            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    TempData["SuccessMessage"] = "If the username exists, the password has been reset.";
                    return RedirectToAction("Index");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password reset successfully. Please log in with your new password.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
    }
}