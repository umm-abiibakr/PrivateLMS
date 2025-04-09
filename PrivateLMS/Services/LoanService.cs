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
        private const int InitialLoanDays = 21; // 3 weeks
        private const int RenewalDays = 7; // 7 days

        public LoanService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoanViewModel>> GetAllLoansAsync()
        {
            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
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
                    IsRenewed = lr.IsRenewed
                })
                .ToListAsync();
        }

        public async Task<LoanViewModel> GetLoanFormAsync(int bookId)
        {
            var book = await _context.Books
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
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == model.BookId);

            if (book == null || !book.IsAvailable || book.AvailableCopies <= 0)
            {
                return false;
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return false;
            }

            var loanRecord = new LoanRecord
            {
                BookId = book.BookId,
                UserId = model.UserId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(InitialLoanDays),
                IsRenewed = false
            };

            book.AvailableCopies--;
            book.IsAvailable = book.AvailableCopies > 0;
            _context.LoanRecords.Add(loanRecord);
            _context.Update(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReturnViewModel> GetReturnFormAsync(int loanRecordId)
        {
            var loanRecord = await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
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
                .Where(lr => lr.User != null && lr.User.Username == username)
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
                    IsRenewed = lr.IsRenewed
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
            return true;
        }
    }
}