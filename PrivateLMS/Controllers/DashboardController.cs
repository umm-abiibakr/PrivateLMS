using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILoanService _loanService;
        private readonly IFineService _fineService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
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

        public async Task<IActionResult> Index(int loansPage = 1, int finesPage = 1, int ratingsPage = 1, int loansPageSize = 5, int finesPageSize = 5, int ratingsPageSize = 5)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to view your dashboard.";
                    return RedirectToAction("Index", "Login");
                }

                var ratingsQuery = _context.BookRatings
                    .Include(br => br.Book)
                    .Where(br => br.UserId == user.Id)
                    .OrderByDescending(br => br.RatedOn);

                var totalRatings = await ratingsQuery.CountAsync();
                var recentRatings = await ratingsQuery
                    .Skip((ratingsPage - 1) * ratingsPageSize)
                    .Take(ratingsPageSize)
                    .Select(br => new BookReviewViewModel
                    {
                        BookId = br.BookId,
                        BookTitle = br.Book.Title,
                        UserName = user.UserName,
                        Rating = br.Rating,
                        Review = br.Review ?? string.Empty,
                        RatedOn = br.RatedOn
                    })
                    .ToListAsync();

                var viewModel = new UserDashboardViewModel
                {
                    ActiveLoans = await _loanService.GetPagedUserActiveLoansAsync(user.UserName, loansPage, loansPageSize),
                    Fines = await _fineService.GetPagedUserFinesAsync(user.UserName, finesPage, finesPageSize, unpaidOnly: true),
                    RecentRatings = new PagedResultViewModel<BookReviewViewModel>
                    {
                        Items = recentRatings,
                        CurrentPage = ratingsPage,
                        PageSize = ratingsPageSize,
                        TotalItems = totalRatings,
                        TotalPages = (int)Math.Ceiling((double)totalRatings / ratingsPageSize)
                    }
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "We couldn't load your dashboard right now. Please try again later.";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
