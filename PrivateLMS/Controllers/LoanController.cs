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

        // GET: Loan
        public async Task<IActionResult> Index()
        {
            try
            {
                var loans = await _context.LoanRecords
                    .Include(lr => lr.Book)
                    .Select(lr => new LoanViewModel
                    {
                        BookId = lr.BookId,
                        BookTitle = lr.Book.Title,
                        LoanerName = lr.LoanerName,
                        LoanerEmail = lr.LoanerEmail,
                        Phone = lr.Phone
                    })
                    .ToListAsync();

                return View(loans);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the loan records: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Loan/Create/5
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
                TempData["ErrorMessage"] = $"An error occurred while loading the loan form: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Loan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(model.BookId);
                model.BookTitle = book?.Title; // Preserve BookTitle on validation failure
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

                book.IsAvailable = false;
                _context.LoanRecords.Add(loanRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully loaned the book: {book.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the loan: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Loan/Return/5
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
                    .Include(lr => lr.Book)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

                if (loanRecord == null)
                {
                    TempData["ErrorMessage"] = $"No loan record found with ID {loanRecordId} to return.";
                    return View("NotFound");
                }

                if (loanRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The loan record for '{loanRecord.Book.Title}' has already been returned.";
                    return View("AlreadyReturned"); // Fixed typo
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
                TempData["ErrorMessage"] = $"An error occurred while loading the return confirmation: {ex.Message}";
                return View("Error");
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
                var loanRecord = await _context.LoanRecords
                    .Include(lr => lr.Book)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == model.LoanRecordId);

                if (loanRecord == null)
                {
                    TempData["ErrorMessage"] = $"No loan record found with ID {model.LoanRecordId} to return.";
                    return View("NotFound");
                }

                if (loanRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The loan record for '{loanRecord.Book.Title}' has already been returned.";
                    return View("AlreadyReturned"); // Fixed typo
                }

                loanRecord.ReturnDate = DateTime.UtcNow;
                loanRecord.Book.IsAvailable = true;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully returned the book: {loanRecord.Book.Title}.";
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