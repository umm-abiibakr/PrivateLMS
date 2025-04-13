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

        public FinesController(
            LibraryDbContext context,
            IFineService fineService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _fineService = fineService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var fines = await _fineService.GetAllFinesAsync();
                return View(fines);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading fines: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> MyFines()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your fines.";
                    return RedirectToAction("Index", "Login");
                }

                var fines = await _fineService.GetUserFinesAsync(user.UserName);
                return View(fines);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your fines: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

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
        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to pay a fine.";
                    return RedirectToAction("Index", "Login");
                }

                var fine = await _context.Fines
                    .Include(f => f.LoanRecord)
                        .ThenInclude(lr => lr.User)
                    .FirstOrDefaultAsync(f => f.Id == id && f.LoanRecord.User.UserName == user.UserName);

                if (fine == null)
                {
                    TempData["ErrorMessage"] = "No fine found, or you do not have permission to pay it.";
                    return RedirectToAction(nameof(MyFines));
                }

                var success = await _fineService.PayFineAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Failed to pay the fine. It may already be paid or not exist.";
                    return RedirectToAction(nameof(MyFines));
                }

                TempData["SuccessMessage"] = "Fine paid successfully.";
                return RedirectToAction(nameof(MyFines));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while paying the fine: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}