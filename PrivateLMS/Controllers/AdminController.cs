using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Data;
using PrivateLMS.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PrivateLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LibraryDbContext _context;

        public AdminController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalBooks = await _context.Books.CountAsync();
            var booksLoanedOut = await _context.LoanRecords
                .CountAsync(l => l.ReturnDate == null);
            var pendingReturns = await _context.LoanRecords
                .CountAsync(l => l.ReturnDate == null);
            var overdueLoans = await _context.LoanRecords
                .CountAsync(l => l.DueDate < DateTime.Now && l.ReturnDate == null);

            var totalUsers = await _context.Users.CountAsync();
            var unapprovedUsers = await _context.Users.CountAsync(u => !u.IsApproved);
            var bannedUsers = await _context.Users.CountAsync(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow);

            var recentLoans = await _context.LoanRecords
                .OrderByDescending(l => l.LoanDate)
                .Take(5)
                .Include(l => l.Book)
                .Include(l => l.User)
                .Select(l => new LoanViewModel
                {
                    LoanRecordId = l.LoanRecordId,
                    BookTitle = l.Book.Title,
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    IsRenewed = l.IsRenewed,
                    LoanerName = l.User.UserName,
                    UserId = l.User.Id
                })
                .ToListAsync();

            var topBorrowedBooks = await _context.LoanRecords
                .Where(l => l.BookId != null)
                .GroupBy(l => new { l.BookId, l.Book.Title })
                .Select(g => new BookViewModel
                {
                    Title = g.Key.Title,
                    TotalLoans = g.Count()
                })
                .OrderByDescending(b => b.TotalLoans)
                .Take(5)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalBooks = totalBooks,
                BooksLoanedOut = booksLoanedOut,
                PendingReturns = pendingReturns,
                OverdueLoans = overdueLoans,
                TotalUsers = totalUsers,
                UnapprovedUsers = unapprovedUsers,
                BannedUsers = bannedUsers,
                RecentLoans = recentLoans,
                TopBorrowedBooks = topBorrowedBooks,
                LoanStats = new LoanStatsViewModel
                {
                    Returned = await _context.LoanRecords.CountAsync(l => l.ReturnDate != null),
                    Pending = await _context.LoanRecords.CountAsync(l => l.ReturnDate == null && l.DueDate >= DateTime.Now),
                    Overdue = await _context.LoanRecords.CountAsync(l => l.DueDate < DateTime.Now && l.ReturnDate == null)
                },
                UserStats = new UserStatsViewModel
                {
                    Approved = await _context.Users.CountAsync(u => u.IsApproved && (!u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow)),
                    Unapproved = await _context.Users.CountAsync(u => !u.IsApproved),
                    Banned = await _context.Users.CountAsync(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow)
                }
            };

            return View(model);
        }
    }
}