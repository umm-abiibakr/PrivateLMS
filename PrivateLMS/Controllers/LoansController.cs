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
    public class LoansController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILoanService _loanService;
        private readonly IFineService _fineService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoansController(
            LibraryDbContext context,
            ILoanService loanService,
            IFineService fineService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _loanService = loanService;
            _fineService = fineService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var loans = await _loanService.GetAllLoansAsync();
                return View(loans);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the loan records: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyLoans()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your loans.";
                    return RedirectToAction("Index", "Login");
                }

                var loans = await _loanService.GetAllUserLoansAsync(user.UserName);
                return View(loans);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your loans: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Create(int? bookId)
        {
            if (!bookId.HasValue)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loaning.";
                return PartialView("_NotFound");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to loan a book.";
                    return RedirectToAction("Index", "Login");
                }

                var loanViewModel = await _loanService.GetLoanFormAsync(bookId.Value);
                if (loanViewModel == null)
                {
                    TempData["ErrorMessage"] = "The book is not available or does not exist.";
                    return View("NotAvailable");
                }

                loanViewModel.UserId = user.Id;
                loanViewModel.LoanerName = $"{user.FirstName} {user.LastName}";
                return View(loanViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the loan form: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var loanViewModel = await _loanService.GetLoanFormAsync(model.BookId);
                if (loanViewModel != null)
                {
                    var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                    model.BookTitle = loanViewModel.BookTitle;
                    model.DueDate = loanViewModel.DueDate;
                    model.LoanerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
                }
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.Id != model.UserId)
                {
                    TempData["ErrorMessage"] = "Invalid user session.";
                    return RedirectToAction("Index", "Login");
                }

                var success = await _loanService.CreateLoanAsync(model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "The book is not available or does not exist.";
                    return View("NotAvailable");
                }

                TempData["SuccessMessage"] = $"Successfully loaned the book: {model.BookTitle}.";
                return RedirectToAction("MyLoans");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the loan: {ex.Message}";
                var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                model.LoanerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
                return View(model);
            }
        }

        public async Task<IActionResult> Return(int? loanRecordId)
        {
            if (!loanRecordId.HasValue)
            {
                TempData["ErrorMessage"] = "Loan Record ID was not provided for returning.";
                return PartialView("_NotFound");
            }

            try
            {
                var returnViewModel = await _loanService.GetReturnFormAsync(loanRecordId.Value);
                if (returnViewModel == null)
                {
                    TempData["ErrorMessage"] = "The loan record does not exist or has already been returned.";
                    return View("AlreadyReturned");
                }

                var user = await _userManager.GetUserAsync(User);
                if (!User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "You do not have permission to return this loan.";
                    return RedirectToAction("MyLoans");
                }

                return View(returnViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the return confirmation: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (!User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "You do not have permission to return this loan.";
                    return RedirectToAction("MyLoans");
                }

                var success = await _loanService.ReturnLoanAsync(model.LoanRecordId);
                if (!success)
                {
                    TempData["ErrorMessage"] = "The loan record does not exist or has already been returned.";
                    return View("AlreadyReturned");
                }

                TempData["SuccessMessage"] = "Successfully returned the book.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the return: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Renew(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Loan ID was not provided for renewal.";
                return PartialView("_NotFound");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to renew a loan.";
                    return RedirectToAction("Index", "Login");
                }

                var loan = await _context.LoanRecords
                    .Include(lr => lr.Book)
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == id.Value && lr.User.UserName == user.UserName);

                if (loan == null)
                {
                    TempData["ErrorMessage"] = "Loan not found or you do not have permission to renew it.";
                    return PartialView("_NotFound");
                }

                if (loan.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = "This loan has already been returned.";
                    return RedirectToAction("MyLoans");
                }

                if (loan.IsRenewed)
                {
                    TempData["ErrorMessage"] = "This loan has already been renewed once and cannot be extended further.";
                    return RedirectToAction("MyLoans");
                }

                var viewModel = new LoanViewModel
                {
                    LoanRecordId = loan.LoanRecordId,
                    BookTitle = loan.Book?.Title ?? "Unknown",
                    UserId = loan.UserId,
                    LoanerName = $"{loan.User.FirstName} {loan.User.LastName}",
                    LoanDate = loan.LoanDate,
                    DueDate = loan.DueDate,
                    IsRenewed = loan.IsRenewed
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the renewal form: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to renew a loan.";
                    return RedirectToAction("Index", "Login");
                }

                var loan = await _context.LoanRecords
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == id && lr.User.UserName == user.UserName);

                if (loan == null)
                {
                    TempData["ErrorMessage"] = "Loan not found or you do not have permission to renew it.";
                    return RedirectToAction("MyLoans");
                }

                var success = await _loanService.RenewLoanAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Failed to renew the loan. It may have been returned or already renewed.";
                    return RedirectToAction("MyLoans");
                }

                TempData["SuccessMessage"] = "Loan renewed successfully for 7 days.";
                return RedirectToAction("MyLoans");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while renewing the loan: {ex.Message}";
                return RedirectToAction("MyLoans");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReturnStatus(int loanRecordId)
        {
            var loan = await _context.LoanRecords.FindAsync(loanRecordId);
            if (loan == null)
            {
                return NotFound();
            }

            if (loan.ReturnDate.HasValue)
            {
                // Unmark as returned
                loan.ReturnDate = null;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // back to loan list
        }

    }
}