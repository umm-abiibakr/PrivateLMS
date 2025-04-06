using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;

namespace PrivateLMS.Controllers
{
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

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
                return View("Error");
            }
        }

        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null || bookId == 0)
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
                return View("Error");
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
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the loan: {ex.Message}";
                return View("Error");
            }
        }

        public async Task<IActionResult> Return(int? loanRecordId)
        {
            if (loanRecordId == null || loanRecordId == 0)
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
                return View("Error");
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

                TempData["SuccessMessage"] = "Successfully returned the book.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the return: {ex.Message}";
                return View("Error");
            }
        }
    }
}