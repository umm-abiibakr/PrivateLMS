using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILoanService _loanService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LibraryDbContext _context;

        public HomeController(
            IBookService bookService,
            ILoanService loanService,
            UserManager<ApplicationUser> userManager,
            LibraryDbContext context)
        {
            _bookService = bookService;
            _loanService = loanService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new HomeViewModel();

                // New Books: 5 most recently added books
                viewModel.NewBooks = await _context.Books
                    .OrderByDescending(b => b.BookId)
                    .Take(5)
                    .Select(b => new BookViewModel
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Description = b.Description,
                        CoverImagePath = b.CoverImagePath,
                        IsAvailable = b.IsAvailable,
                        AvailableCopies = b.AvailableCopies
                    })
                    .ToListAsync();

                // Popular Books: 5 books with the most loans or highest available copies
                viewModel.PopularBooks = await _context.Books
                    .Select(b => new
                    {
                        Book = b,
                        LoanCount = _context.LoanRecords.Count(lr => lr.BookId == b.BookId)
                    })
                    .OrderByDescending(x => x.LoanCount)
                    .ThenByDescending(x => x.Book.AvailableCopies)
                    .Take(5)
                    .Select(x => new BookViewModel
                    {
                        BookId = x.Book.BookId,
                        Title = x.Book.Title,
                        Description = x.Book.Description,
                        CoverImagePath = x.Book.CoverImagePath,
                        IsAvailable = x.Book.IsAvailable,
                        AvailableCopies = x.Book.AvailableCopies
                    })
                    .ToListAsync();

                // Book Reviews: 5 most recent reviews with book and user info
                viewModel.RecentReviews = await _context.BookRatings
                    .Include(br => br.Book)
                    .Include(br => br.User)
                    .OrderByDescending(br => br.RatedOn)
                    .Take(5)
                    .Select(br => new BookReviewViewModel
                    {
                        BookId = br.BookId,
                        BookTitle = br.Book.Title,
                        UserName = br.User.UserName,
                        Rating = br.Rating,
                        RatedOn = br.RatedOn
                    })
                    .ToListAsync();

                // Overdue Loans and Recommendations: For logged-in users
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        // Overdue Loans
                        viewModel.OverdueLoans = await _loanService.GetOverdueLoansAsync(user.UserName);

                        // Recommendations
                        viewModel.Recommendations = await _bookService.GetRecommendedBooksAsync(user.Id);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the homepage: {ex.Message}";
                return RedirectToAction("Error");
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}