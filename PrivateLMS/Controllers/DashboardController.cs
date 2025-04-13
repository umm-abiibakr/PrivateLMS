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

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view your dashboard.";
                    return RedirectToAction("Index", "Login");
                }

                var viewModel = new UserDashboardViewModel
                {
                    ActiveLoans = await _loanService.GetUserActiveLoansAsync(user.UserName),
                    Fines = (await _fineService.GetUserFinesAsync(user.UserName))
                        .Where(f => !f.IsPaid)
                        .ToList(),
                    RecentRatings = await _context.BookRatings
                        .Include(br => br.Book)
                        .Where(br => br.UserId == user.Id)
                        .OrderByDescending(br => br.RatedOn)
                        .Take(5)
                        .Select(br => new BookReviewViewModel
                        {
                            BookId = br.BookId,
                            BookTitle = br.Book.Title,
                            UserName = user.UserName,
                            Rating = br.Rating,
                            RatedOn = br.RatedOn
                        })
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading your dashboard: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}