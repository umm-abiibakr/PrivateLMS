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

        public async Task<IActionResult> Index(int? userId, string actionType)
        {
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

            var activities = await query.OrderByDescending(ua => ua.Timestamp).ToListAsync();
            ViewBag.UserId = userId;
            ViewBag.ActionType = actionType;
            ViewBag.ActionTypes = new[] { "Login", "Ban", "Unban", "LoanBook", "ReturnBook", "RateBook" };
            return View(activities);
        }
    }
}