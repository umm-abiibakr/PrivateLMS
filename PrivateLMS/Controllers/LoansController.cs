using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor

namespace PrivateLMS.Controllers
{
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoansController(ILoanService loanService, IHttpContextAccessor httpContextAccessor)
        {
            _loanService = loanService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Loan
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

        // GET: Loan/MyLoans
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

        // GET: Loan/Create/5
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loaning.";
                return View("NotFound");
            }

            try
            {
                var loanViewModel = await _loanService.GetLoanFormAsync(bookId.Value);
                if (loanViewModel == null)
                {
                    TempData["ErrorMessage"] = "The book is not available or does not exist.";
                    return View("NotAvailable");
                }
                return View(loanViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the loan form: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Loan/Create
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
                }
                return View(model);
            }

            try
            {
                var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
                if (!string.IsNullOrEmpty(username))
                {
                    model.LoanerName = username; // Auto-set LoanerName from session
                }

                var success = await _loanService.CreateLoanAsync(model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "The book is not available or does not exist.";
                    return View("NotAvailable");
                }

                TempData["SuccessMessage"] = $"Successfully loaned the book: {model.BookTitle}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the loan: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Loan/Return/5
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

        // POST: Loan/Return/5
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

                TempData["SuccessMessage"] = "Successfully returned the book.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the return: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}