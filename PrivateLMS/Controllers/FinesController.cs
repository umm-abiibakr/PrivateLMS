using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize]
    public class FinesController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IFineService _fineService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public FinesController(
            LibraryDbContext context,
            IFineService fineService,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _fineService = fineService;
            _userManager = userManager;
            _emailService = emailService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var pagedFines = await _fineService.GetPagedAllFinesAsync(page, pageSize);
                return View(pagedFines);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading fines: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyFines(int page = 1, int pageSize = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your fines.";
                    return RedirectToAction("Index", "Login");
                }

                var pagedFines = await _fineService.GetPagedUserFinesAsync(user.UserName, page, pageSize);
                return View(pagedFines);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your fines: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Pay(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Fine ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to pay a fine.";
                    return RedirectToAction("Index", "Login");
                }

                var fine = await _fineService.GetFineByIdAsync(id.Value);
                if (fine == null || fine.LoanerName != $"{user.FirstName} {user.LastName}")
                {
                    TempData["ErrorMessage"] = "No fine found, or you do not have permission to pay it.";
                    return PartialView("_NotFound");
                }

                if (fine.IsPaid)
                {
                    TempData["ErrorMessage"] = "This fine has already been paid.";
                    return RedirectToAction(nameof(MyFines));
                }

                var payFineViewModel = new PayFineViewModel
                {
                    FineId = fine.Id,
                    BookTitle = fine.BookTitle,
                    LoanerName = fine.LoanerName,
                    Amount = fine.Amount,
                    IssuedDate = fine.IssuedDate
                };
                return View(payFineViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the fine: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to request fine payment.";
                    return RedirectToAction("Index", "Login");
                }

                var fine = await _context.Fines
                    .Include(f => f.LoanRecord)
                        .ThenInclude(lr => lr.User)
                    .Include(f => f.LoanRecord.Book)
                    .FirstOrDefaultAsync(f => f.Id == id && f.LoanRecord.User.UserName == user.UserName);

                if (fine == null)
                {
                    TempData["ErrorMessage"] = "Fine not found or you do not have permission to request payment.";
                    return RedirectToAction(nameof(MyFines));
                }

                // Log the request
                _context.UserActivities.Add(new UserActivity
                {
                    UserId = fine.UserId,
                    Action = "RequestFinePayment",
                    Timestamp = DateTime.UtcNow,
                    Details = $"User requested to pay fine of NGN {fine.Amount} for book '{fine.LoanRecord.Book?.Title}'"
                });

                // Send admin email
                var adminEmail = "admin@warathatulambiya.com"; 
                var subject = "Fine Payment Request";
                var body = $"User {user.FirstName} {user.LastName} ({user.Email}) has requested to pay a fine of NGN {fine.Amount} for the book '{fine.LoanRecord.Book?.Title}' (Loan ID: {fine.LoanId}).";

                await _emailService.SendEmailAsync(adminEmail, subject, body);

                TempData["SuccessMessage"] = "Your request has been sent to the admin. You will be contacted shortly.";
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyFines));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing your request: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePaidStatus(int id)
        {
            var fine = await _context.Fines.FindAsync(id);

            if (fine == null)
            {
                TempData["ErrorMessage"] = "Fine not found.";
                return RedirectToAction("Index");
            }

            fine.IsPaid = !fine.IsPaid;
            _context.Fines.Update(fine);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Fine marked as {(fine.IsPaid ? "Paid" : "Unpaid")}.";
            return RedirectToAction("Index");
        }
    }
}