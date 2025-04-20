using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public class LoanService : ILoanService
    {
        private readonly LibraryDbContext _context;
        private readonly IFineService _fineService;
        private const int InitialLoanDays = 21; // 3 weeks
        private const int RenewalDays = 7; // 7 days

        public LoanService(LibraryDbContext context, IFineService fineService)
        {
            _context = context;
            _fineService = fineService;
        }

        public async Task<List<LoanViewModel>> GetAllLoansAsync()
        {
            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .AsNoTracking()
                .Select(lr => new LoanViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    UserId = lr.UserId,
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    IsRenewed = lr.IsRenewed,
                    DaysOverdue = lr.DueDate.HasValue && lr.ReturnDate == null && lr.DueDate < DateTime.UtcNow ? (int)(DateTime.UtcNow - lr.DueDate.Value).TotalDays : 0
                })
                .ToListAsync();
        }

        public async Task<LoanViewModel?> GetLoanFormAsync(int bookId)
        {
            var book = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null || !book.IsAvailable || book.AvailableCopies <= 0)
            {
                return null;
            }

            return new LoanViewModel
            {
                BookId = book.BookId,
                BookTitle = book.Title ?? string.Empty,
                DueDate = DateTime.UtcNow.AddDays(InitialLoanDays)
            };
        }

        public async Task<bool> CreateLoanAsync(LoanViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books.FindAsync(model.BookId);
                if (book == null || !book.IsAvailable)
                {
                    return false;
                }

                var activeLoanCount = await _context.LoanRecords
                    .Where(lr => lr.UserId == model.UserId && lr.ReturnDate == null)
                    .CountAsync();
                if (activeLoanCount >= 3)
                {
                    return false;
                }

                var loanRecord = new LoanRecord
                {
                    BookId = model.BookId,
                    UserId = model.UserId,
                    LoanDate = DateTime.UtcNow,
                    DueDate = model.DueDate,
                    IsRenewed = false
                };

                book.IsAvailable = false;
                _context.LoanRecords.Add(loanRecord);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ReturnViewModel?> GetReturnFormAsync(int loanRecordId)
        {
            var loanRecord = await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loanRecord == null || loanRecord.ReturnDate != null)
            {
                return null;
            }

            return new ReturnViewModel
            {
                LoanRecordId = loanRecord.LoanRecordId,
                BookTitle = loanRecord.Book?.Title ?? string.Empty,
                UserId = loanRecord.UserId,
                LoanerName = loanRecord.User != null ? $"{loanRecord.User.FirstName} {loanRecord.User.LastName}" : "Unknown",
                LoanDate = loanRecord.LoanDate,
                DueDate = loanRecord.DueDate
            };
        }

        public async Task<bool> ReturnLoanAsync(int loanRecordId)
        {
            var loanRecord = await _context.LoanRecords
                .Include(lr => lr.Book)
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loanRecord == null || loanRecord.ReturnDate != null)
            {
                return false;
            }

            loanRecord.ReturnDate = DateTime.UtcNow;
            if (loanRecord.Book != null)
            {
                loanRecord.Book.AvailableCopies++;
                loanRecord.Book.IsAvailable = true;
            }
            _context.Update(loanRecord);
            await _context.SaveChangesAsync();

            // Trigger fine calculation
            await _fineService.UpdateFineAsync(loanRecordId);

            // Log the return action
            _context.UserActivities.Add(new UserActivity
            {
                UserId = loanRecord.UserId,
                Action = "ReturnBook",
                Timestamp = DateTime.UtcNow,
                Details = $"User returned book ID {loanRecord.BookId} (Title: {loanRecord.Book.Title})"
            });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<LoanViewModel>> GetUserLoansAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<LoanViewModel>();
            }

            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .Where(lr => lr.User != null && lr.User.UserName == username && lr.ReturnDate == null)
                .AsNoTracking()
                .Select(lr => new LoanViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    UserId = lr.UserId,
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    IsRenewed = lr.IsRenewed,
                    DaysOverdue = lr.DueDate.HasValue && lr.ReturnDate == null && lr.DueDate < DateTime.UtcNow ? (int)(DateTime.UtcNow - lr.DueDate.Value).TotalDays : 0
                })
                .ToListAsync();
        }

        public async Task<List<LoanViewModel>> GetAllUserLoansAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<LoanViewModel>();
            }

            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .Where(lr => lr.User != null && lr.User.UserName == username)
                .OrderByDescending(lr => lr.LoanDate)
                .AsNoTracking()
                .Select(lr => new LoanViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    UserId = lr.UserId,
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    IsRenewed = lr.IsRenewed,
                    DaysOverdue = lr.DueDate.HasValue && lr.ReturnDate == null && lr.DueDate < DateTime.UtcNow ? (int)(DateTime.UtcNow - lr.DueDate.Value).TotalDays : 0
                })
                .ToListAsync();
        }

        public async Task<List<LoanViewModel>> GetOverdueLoansAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<LoanViewModel>();
            }

            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .Where(lr => lr.User != null && lr.User.UserName == username && lr.ReturnDate == null && lr.DueDate.HasValue && lr.DueDate < DateTime.UtcNow)
                .AsNoTracking()
                .Select(lr => new LoanViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    UserId = lr.UserId,
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    IsRenewed = lr.IsRenewed,
                    DaysOverdue = lr.DueDate.HasValue ? (int)(DateTime.UtcNow - lr.DueDate.Value).TotalDays : 0
                })
                .ToListAsync();
        }

        public async Task<List<LoanViewModel>> GetUserActiveLoansAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<LoanViewModel>();
            }

            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .Where(lr => lr.User != null && lr.User.UserName == username && lr.ReturnDate == null)
                .AsNoTracking()
                .Select(lr => new LoanViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    UserId = lr.UserId,
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    IsRenewed = lr.IsRenewed,
                    DaysOverdue = lr.DueDate.HasValue && lr.DueDate < DateTime.UtcNow ? (int)(DateTime.UtcNow - lr.DueDate.Value).TotalDays : 0
                })
                .ToListAsync();
        }

        public async Task<bool> RenewLoanAsync(int loanRecordId)
        {
            var loanRecord = await _context.LoanRecords
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loanRecord == null || loanRecord.ReturnDate != null || loanRecord.IsRenewed || !loanRecord.DueDate.HasValue)
            {
                return false; // Cannot renew if returned, already renewed, or no due date
            }

            loanRecord.DueDate = loanRecord.DueDate.Value.AddDays(RenewalDays);
            loanRecord.IsRenewed = true;
            _context.Update(loanRecord);
            await _context.SaveChangesAsync();

            // Log the renewal action
            var book = await _context.Books.FindAsync(loanRecord.BookId);
            _context.UserActivities.Add(new UserActivity
            {
                UserId = loanRecord.UserId,
                Action = "RenewLoan",
                Timestamp = DateTime.UtcNow,
                Details = $"User renewed loan for book ID {loanRecord.BookId} (Title: {book?.Title ?? "Unknown"})"
            });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetActiveLoanCountAsync(int userId)
        {
            return await _context.LoanRecords
                .Where(lr => lr.UserId == userId && lr.ReturnDate == null)
                .CountAsync();
        }
    }
}