using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserActivitiesController : Controller
    {
        private readonly LibraryDbContext _context;

        public UserActivitiesController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? userId, string actionType, int pageNumber = 1)
        {
            const int pageSize = 10;

            var query = _context.UserActivities
                .Include(ua => ua.User)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(ua => ua.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(ua => ua.Action == actionType);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var activities = await query
                .OrderByDescending(ua => ua.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new PagedResultViewModel<UserActivity>
            {
                Items = activities,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            ViewBag.UserId = userId;
            ViewBag.ActionType = actionType;
            ViewBag.ActionTypes = new[] {
                "Login", "Logout", "FailedLogin", "Ban", "Unban",
                "LoanBook", "ReturnBook", "RenewLoan", "RequestLoan",
                "RateBook", "Register", "ApproveMembership", "RejectMembership",
                "TerminateMembership", "IncurFine", "PayFine", "WaiveFine",
                "AddBook", "UpdateBook", "RemoveBook", "SubmitRecommendationFeedback"
            };

            return View(viewModel);
        }
    }
}