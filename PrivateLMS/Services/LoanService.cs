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
                    BookTitle = lr.Book.Title,
                    LoanerName = lr.LoanerName,
                    LoanerEmail = lr.LoanerEmail,
                    Phone = lr.Phone
                })
                .ToListAsync();
        }

        public async Task<LoanViewModel> GetLoanFormAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || !book.IsAvailable)
            {
                return null; // Let controller handle null case
            }

            return new LoanViewModel
            {
                BookId = book.BookId,
                BookTitle = book.Title
            };
        }

        public async Task<bool> CreateLoanAsync(LoanViewModel model)
        {
            var book = await _context.Books.FindAsync(model.BookId);
            if (book == null || !book.IsAvailable)
            {
                return false;
            }

            var loanRecord = new LoanRecord
            {
                BookId = book.BookId,
                LoanerName = model.LoanerName,
                LoanerEmail = model.LoanerEmail,
                Phone = model.Phone,
                LoanDate = DateTime.UtcNow
            };

            book.IsAvailable = false;
            _context.LoanRecords.Add(loanRecord);
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
                return null; // Let controller handle null case
            }

            return new ReturnViewModel
            {
                LoanRecordId = loanRecord.LoanRecordId,
                BookTitle = loanRecord.Book.Title,
                LoanerName = loanRecord.LoanerName,
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
            loanRecord.Book.IsAvailable = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}