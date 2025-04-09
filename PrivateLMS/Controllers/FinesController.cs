using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    //[Authorize]
    public class FinesController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IFineService _fineService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FinesController(LibraryDbContext context, IFineService fineService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _fineService = fineService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Fines (Admin view of all fines)
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

        // GET: Fines/MyFines (User view of their fines)
        public async Task<IActionResult> MyFines()
        {
            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your fines.";
                    return RedirectToAction("Index", "Login");
                }

                var fines = await _fineService.GetUserFinesAsync(username);
                return View(fines);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your fines: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Fines/Pay/5
        public async Task<IActionResult> Pay(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Fine ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to pay a fine.";
                    return RedirectToAction("Index", "Login");
                }

                var loan = await _context.LoanRecords
                    .Include(lr => lr.Book)
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == id.Value && lr.User.Username == username);

                if (loan == null)
                {
                    TempData["ErrorMessage"] = "No fine found for this loan, or you do not have permission to pay it.";
                    return View("NotFound");
                }

                if (loan.IsFinePaid)
                {
                    TempData["ErrorMessage"] = "This fine has already been paid.";
                    return RedirectToAction(nameof(MyFines));
                }

                var fine = new FineViewModel
                {
                    LoanRecordId = loan.LoanRecordId,
                    BookTitle = loan.Book?.Title ?? "Unknown",
                    LoanerName = loan.User != null ? $"{loan.User.FirstName} {loan.User.LastName}" : "Unknown",
                    LoanDate = loan.LoanDate,
                    DueDate = loan.DueDate,
                    ReturnDate = loan.ReturnDate,
                    FineAmount = loan.FineAmount,
                    IsFinePaid = loan.IsFinePaid
                };
                return View(fine);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the fine: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Fines/Pay/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to pay a fine.";
                    return RedirectToAction("Index", "Login");
                }

                var loan = await _context.LoanRecords
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == id && lr.User.Username == username);

                if (loan == null)
                {
                    TempData["ErrorMessage"] = "No fine found for this loan, or you do not have permission to pay it.";
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