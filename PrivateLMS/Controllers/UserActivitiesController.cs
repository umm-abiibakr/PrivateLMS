using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index(int? userId = null, string actionType = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.UserActivities
                .Include(ua => ua.User)
                .AsNoTracking();

            // Apply filters
            if (userId.HasValue)
            {
                query = query.Where(ua => ua.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(ua => ua.Action == actionType);
            }

            // Get action types for filter dropdown
            ViewBag.ActionTypes = await _context.UserActivities
                .Select(ua => ua.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
            ViewBag.UserId = userId;
            ViewBag.ActionType = actionType;

            // Pagination
            var totalItems = await query.CountAsync();
            var activities = await query
                .OrderByDescending(ua => ua.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ua => new UserActivityViewModel
                {
                    Id = ua.Id,
                    UserId = ua.UserId,
                    UserName = ua.User != null ? ua.User.UserName : "Unknown",
                    Action = ua.Action,
                    Timestamp = ua.Timestamp,
                    Details = ua.Details
                })
                .ToListAsync();

            var model = new PagedResultViewModel<UserActivityViewModel>
            {
                Items = activities,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(model);
        }
    }
}