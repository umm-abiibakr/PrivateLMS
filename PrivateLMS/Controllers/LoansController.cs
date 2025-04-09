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
    public class LoansController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILoanService _loanService;
        private readonly IFineService _fineService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoansController(LibraryDbContext context, ILoanService loanService, IFineService fineService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _loanService = loanService;
            _fineService = fineService;
            _httpContextAccessor = httpContextAccessor;
        }

        //[Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> MyLoans()
        {
            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your loans.";
                    return RedirectToAction("Index", "Login");
                }

                var loans = await _loanService.GetUserLoansAsync(username);
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
            if (bookId == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loaning.";
                return View("NotFound");
            }

            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to loan a book.";
                    return RedirectToAction("Index", "Login");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "Login");
                }

                var loanViewModel = await _loanService.GetLoanFormAsync(bookId.Value);
                if (loanViewModel == null)
                {
                    TempData["ErrorMessage"] = "The book is not available or does not exist.";
                    return View("NotAvailable");
                }
                loanViewModel.UserId = user.UserId;
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
                    model.BookTitle = loanViewModel.BookTitle;
                    model.DueDate = loanViewModel.DueDate;
                    var user = await _context.Users.FindAsync(model.UserId);
                    model.LoanerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
                }
                return View(model);
            }

            try
            {
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
                return RedirectToAction("Error", "Home");
            }
        }
        public async Task<IActionResult> Return(int? loanRecordId)
        {
            if (loanRecordId == null)
            {
                TempData["ErrorMessage"] = "Loan Record ID was not provided for returning.";
                return View("NotFound");
            }

            try
            {
                var returnViewModel = await _loanService.GetReturnFormAsync(loanRecordId.Value);
                if (returnViewModel == null)
                {
                    TempData["ErrorMessage"] = "The loan record does not exist or has already been returned.";
                    return View("AlreadyReturned");
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
                var success = await _loanService.ReturnLoanAsync(model.LoanRecordId);
                if (!success)
                {
                    TempData["ErrorMessage"] = "The loan record does not exist or has already been returned.";
                    return View("AlreadyReturned");
                }

                await _fineService.UpdateFineAsync(model.LoanRecordId);

                TempData["SuccessMessage"] = "Successfully returned the book.";
                return RedirectToAction("MyLoans");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the return: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Loans/Renew/5
        public async Task<IActionResult> Renew(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Loan ID was not provided for renewal.";
                return View("NotFound");
            }

            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to renew a loan.";
                    return RedirectToAction("Index", "Login");
                }

                var loan = await _context.LoanRecords
                    .Include(lr => lr.Book)
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == id.Value && lr.User.Username == username);

                if (loan == null)
                {
                    TempData["ErrorMessage"] = "Loan not found or you do not have permission to renew it.";
                    return View("NotFound");
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
                    LoanerName = loan.User != null ? $"{loan.User.FirstName} {loan.User.LastName}" : "Unknown",
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

        // POST: Loans/Renew/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id)
        {
            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to renew a loan.";
                    return RedirectToAction("Index", "Login");
                }

                var loan = await _context.LoanRecords
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == id && lr.User.Username == username);

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
    }
}