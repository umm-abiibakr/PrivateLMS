using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;

namespace PrivateLMS.Controllers
{
    public class LoanController : Controller
    {
        private readonly LibraryDbContext _context;

        public LoanController(LibraryDbContext context)
        {
            _context = context;
        }

        // Displays the loan form for a specific book.
        // GET: loan/Create/5
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null || bookId == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loaning.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {bookId} to loan.";
                    return View("NotFound");
                }

                if (!book.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"The book '{book.Title}' is currently not available for loaning.";
                    return View("NotAvailable");
                }

                var loanViewModel = new LoanViewModel
                {
                    BookId = book.BookId,
                    BookTitle = book.Title
                };

                return View(loanViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the loan form.";
                return View("Error");
            }
        }

        // Processes the loaning action, creates a loanRecord, updates the book's availability
        // POST: loan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var book = await _context.Books.FindAsync(model.BookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {model.BookId} to loan.";
                    return View("NotFound");
                }

                if (!book.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"The book '{book.Title}' is already loaned.";
                    return View("NotAvailable");
                }

                var loanRecord = new LoanRecord
                {
                    BookId = book.BookId,
                    LoanerName = model.LoanerName,
                    LoanerEmail = model.LoanerEmail,
                    Phone = model.Phone,
                    LoanDate = DateTime.UtcNow
                };

                // Update the book's availability
                book.IsAvailable = false;

                _context.LoanRecords.Add(loanRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully loaned the book: {book.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the loaning action.";
                return View("Error");
            }
        }

        // Displays the return confirmation for a specific loan record
        // GET: loan/Return/5
        public async Task<IActionResult> Return(int? loanRecordId)
        {
            if (loanRecordId == null || loanRecordId == 0)
            {
                TempData["ErrorMessage"] = "Loan Record ID was not provided for returning.";
                return View("NotFound");
            }

            try
            {
                var loanRecord = await _context.LoanRecords
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.LoanRecordId == loanRecordId);

                if (loanRecord == null)
                {
                    TempData["ErrorMessage"] = $"No loan record found with ID {loanRecordId} to return.";
                    return View("NotFound");
                }

                if (loanRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The loan record for '{loanRecord.Book.Title}' has already been returned.";
                    return View("AbreadyReturned");
                }

                var returnViewModel = new ReturnViewModel
                {
                    LoanRecordId = loanRecord.LoanRecordId,
                    BookTitle = loanRecord.Book.Title,
                    LoanerName = loanRecord.LoanerName,
                    LoanDate = loanRecord.LoanDate
                };

                return View(returnViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the return confirmation.";
                return View("Error");
            }
        }

        // Processes the return action, updates the loanRecord with the return date, updates the book's availability
        // POST: loan/Return/5
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
                var loanRecord = await _context.LoanRecords
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.LoanRecordId == model.LoanRecordId);

                if (loanRecord == null)
                {
                    TempData["ErrorMessage"] = $"No loan record found with ID {model.LoanRecordId} to return.";
                    return View("NotFound");
                }

                if (loanRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The loan record for '{loanRecord.Book.Title}' has already been returned.";
                    return View("AbreadyReturned");
                }

                // Update the loan record
                loanRecord.ReturnDate = DateTime.UtcNow;

                // Update the book's availability
                loanRecord.Book.IsAvailable = true;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully returned the book: {loanRecord.Book.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the return action.";
                return View("Error");
            }
        }
    }
}

