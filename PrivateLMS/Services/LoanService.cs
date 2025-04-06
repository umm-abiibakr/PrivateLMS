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

        public LoanService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoanViewModel>> GetAllLoansAsync()
        {
            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Select(lr => new LoanViewModel
                {
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    LoanerName = lr.LoanerName ?? string.Empty,
                    LoanerEmail = lr.LoanerEmail ?? string.Empty,
                    Phone = lr.Phone ?? string.Empty,
                    ReturnDate = lr.ReturnDate
                })
                .ToListAsync();
        }

        public async Task<LoanViewModel> GetLoanFormAsync(int bookId)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null || !book.IsAvailable)
            {
                return null;
            }

            return new LoanViewModel
            {
                BookId = book.BookId,
                BookTitle = book.Title ?? string.Empty
            };
        }

        public async Task<bool> CreateLoanAsync(LoanViewModel model)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == model.BookId);

            if (book == null || !book.IsAvailable)
            {
                return false;
            }

            var loanRecord = new LoanRecord
            {
                BookId = book.BookId,
                LoanerName = model.LoanerName ?? string.Empty,
                LoanerEmail = model.LoanerEmail ?? string.Empty,
                Phone = model.Phone ?? string.Empty,
                LoanDate = DateTime.UtcNow
            };

            book.IsAvailable = false;
            _context.LoanRecords.Add(loanRecord);
            _context.Update(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReturnViewModel> GetReturnFormAsync(int loanRecordId)
        {
            var loanRecord = await _context.LoanRecords
                .Include(lr => lr.Book)
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loanRecord == null || loanRecord.ReturnDate != null)
            {
                return null;
            }

            return new ReturnViewModel
            {
                LoanRecordId = loanRecord.LoanRecordId,
                BookTitle = loanRecord.Book != null ? loanRecord.Book.Title ?? string.Empty : string.Empty,
                LoanerName = loanRecord.LoanerName ?? string.Empty,
                LoanDate = loanRecord.LoanDate
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
                .Where(lr => lr.LoanerName == username) // Assuming LoanerName matches session Username
                .Select(lr => new LoanViewModel
                {
                    BookId = lr.BookId,
                    BookTitle = lr.Book != null ? lr.Book.Title ?? "Unknown" : "Unknown",
                    LoanerName = lr.LoanerName ?? string.Empty,
                    LoanerEmail = lr.LoanerEmail ?? string.Empty,
                    Phone = lr.Phone ?? string.Empty,
                    ReturnDate = lr.ReturnDate,
                    LoanDate = lr.LoanDate 
                })
                .ToListAsync();
        }
    }
}