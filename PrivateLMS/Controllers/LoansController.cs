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
        private readonly IEmailService _emailService;

        public LoansController(
            LibraryDbContext context,
            ILoanService loanService,
            IFineService fineService,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _loanService = loanService;
            _fineService = fineService;
            _userManager = userManager;
            _emailService = emailService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var pagedLoans = await _loanService.GetPagedAllLoansAsync(page, pageSize);
                return View(pagedLoans);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the loan records: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyLoans(int activePage = 1, int pastPage = 1, int activePageSize = 5, int pastPageSize = 5)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your loans.";
                    return RedirectToAction("Index", "Login");
                }

                var activeLoans = await _loanService.GetPagedUserActiveLoansAsync(user.UserName, activePage, activePageSize);
                var allUserLoans = await _loanService.GetPagedAllUserLoansAsync(user.UserName, pastPage, pastPageSize);
                var pastLoans = allUserLoans.Items
                    .Where(lr => lr.ReturnDate != null)
                    .ToList();

                var pastLoansPaged = new PagedResultViewModel<LoanViewModel>
                {
                    Items = pastLoans,
                    CurrentPage = pastPage,
                    PageSize = pastPageSize,
                    TotalItems = allUserLoans.TotalItems - activeLoans.TotalItems, // Approximate count of past loans
                    TotalPages = (int)Math.Ceiling((double)(allUserLoans.TotalItems - activeLoans.TotalItems) / pastPageSize)
                };

                var viewModel = new MyLoansViewModel
                {
                    ActiveLoans = activeLoans,
                    PastLoans = pastLoansPaged
                };

                return View(viewModel);
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
                    TempData["ErrorMessage"] = "You must be logged in to request a loan.";
                    return RedirectToAction("Index", "Login");
                }

                // Check active loan count
                var activeLoanCount = await _loanService.GetActiveLoanCountAsync(user.Id);
                if (activeLoanCount >= 2)
                {
                    TempData["ErrorMessage"] = "You cannot borrow more than two books at a time.";
                    return RedirectToAction("MyLoans");
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

                // Check active loan count
                var activeLoanCount = await _loanService.GetActiveLoanCountAsync(user.Id);
                if (activeLoanCount >= 2)
                {
                    TempData["ErrorMessage"] = "You cannot borrow more than two books at a time.";
                    return RedirectToAction("MyLoans");
                }

                // Fetch the book title from the database to ensure it's not null
                var book = await _context.Books.FindAsync(model.BookId);
                if (book != null)
                {
                    model.BookTitle = book.Title;
                }
                else
                {
                    model.BookTitle = "Unknown Book"; // Fallback in case book is not found
                }

                var success = await _loanService.CreateLoanAsync(model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "The book is not available or does not exist.";
                    return View("NotAvailable");
                }

                // Send email notification to user
                var emailBody = $@"
                    <h2>Book Loan Confirmation</h2>
                    <p>Assalamu Alaykum {user.FirstName} {user.LastName},</p>
                    <p>You have requested to loan the following book:</p>
                    <ul>
                        <li><strong>Book Title:</strong> {model.BookTitle}</li>
                        <li><strong>Loan Date:</strong> {DateTime.UtcNow.ToString("MMMM dd, yyyy")}</li>
                        <li><strong>Due Date:</strong> {model.DueDate.ToString()}</li>
                    </ul>
                    <p>You will soon be contactted by the Admin team bi idhnillaah.</p>
                    <p>Ensure to preserve the book and return it by the due date to avoid fines.</p>
                    <p>Baarakallaahu Feekum,<br/>Admin@WarathatulAmbiya</p>";
                await _emailService.SendEmailAsync(user.Email, "Book Loan Confirmation", emailBody);

                // Send email notification to admin
                var adminEmailBody = $@"
                    <h2>New Loan Request Notification</h2>
                    <p>A new book loan request has been submitted.</p>
                    <p><strong>Details:</strong></p>
                    <ul>
                    <li><strong>Member Name:</strong> {user.FirstName} {user.LastName}</li>
                    <li><strong>Member Email:</strong> {user.Email}</li>
                    <li><strong>Book Title:</strong> {model.BookTitle}</li>
                    <li><strong>Loan Date:</strong> {DateTime.UtcNow.ToString("MMMM dd, yyyy")}</li>
                    <li><strong>Due Date:</strong> {model.DueDate.ToString()}</li>
                    </ul>
                    <p>Please review the request in the Library Management System.</p>
                    <p>Baarakallaahu Feekum,<br/>Warathatul Ambiya Library System</p>";
                await _emailService.SendEmailAsync("admin@warathatulambiya.com", "New Loan Request", adminEmailBody);

                TempData["SuccessMessage"] = $"Successfully requested a loan: {model.BookTitle}. " +
                    $"A confirmation email has been sent, and an admin will get back to you shortly bi idhnillaah.";
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

                // Fetch loan details to get user and book information
                var loan = await _context.LoanRecords
                    .Include(lr => lr.User)
                    .Include(lr => lr.Book)
                    .FirstOrDefaultAsync(lr => lr.LoanRecordId == model.LoanRecordId);

                if (loan == null)
                {
                    TempData["ErrorMessage"] = "Loan record not found.";
                    return View("AlreadyReturned");
                }

                // Send confirmation email to user
                var userEmailBody = $@"
                    <h2>Book Return Confirmation</h2>
                    <p>Dear {loan.User.FirstName} {loan.User.LastName},</p>
                    <p>You have successfully returned the following book:</p>
                    <ul>
                    <li><strong>Book Title:</strong> {loan.Book.Title}</li>
                    <li><strong>Return Date:</strong> {DateTime.UtcNow.ToString("MMMM dd, yyyy")}</li>
                    </ul>
                    <p>Please check your account for any applicable fines if the book was returned late.</p>
                    <p>Baarakallaahu Feekum,<br/>Admin@WarathatulAmbiya</p>";

                await _emailService.SendEmailAsync(loan.User.Email, "Book Return Confirmation", userEmailBody);

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
            return RedirectToAction(nameof(Index));
        }
    }
} 