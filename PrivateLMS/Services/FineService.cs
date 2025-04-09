using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public class FineService : IFineService
    {
        private readonly LibraryDbContext _context;
        private const decimal BaseFine = 1000m; // 1000 NGN starting fine
        private const decimal DailyFineRate = 1000m; // 1000 NGN per day late

        public FineService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<FineViewModel>> GetAllFinesAsync()
        {
            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .Where(lr => lr.FineAmount > 0)
                .Select(lr => new FineViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookTitle = lr.Book.Title ?? "Unknown",
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    FineAmount = lr.FineAmount,
                    IsFinePaid = lr.IsFinePaid
                })
                .ToListAsync();
        }

        public async Task<List<FineViewModel>> GetUserFinesAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<FineViewModel>();
            }

            return await _context.LoanRecords
                .Include(lr => lr.Book)
                .Include(lr => lr.User)
                .Where(lr => lr.User != null && lr.User.Username == username && lr.FineAmount > 0)
                .Select(lr => new FineViewModel
                {
                    LoanRecordId = lr.LoanRecordId,
                    BookTitle = lr.Book.Title ?? "Unknown",
                    LoanerName = lr.User != null ? $"{lr.User.FirstName} {lr.User.LastName}" : "Unknown",
                    LoanDate = lr.LoanDate,
                    DueDate = lr.DueDate,
                    ReturnDate = lr.ReturnDate,
                    FineAmount = lr.FineAmount,
                    IsFinePaid = lr.IsFinePaid
                })
                .ToListAsync();
        }

        public async Task<decimal> CalculateFineAsync(int loanRecordId)
        {
            var loan = await _context.LoanRecords
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loan == null || !loan.DueDate.HasValue || loan.ReturnDate == null || loan.ReturnDate <= loan.DueDate)
            {
                return 0m;
            }

            var daysLate = (loan.ReturnDate.Value - loan.DueDate.Value).Days;
            return daysLate > 0 ? BaseFine + (daysLate * DailyFineRate) : 0m;
        }

        public async Task<bool> UpdateFineAsync(int loanRecordId)
        {
            var loan = await _context.LoanRecords
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loan == null) return false;

            loan.FineAmount = await CalculateFineAsync(loanRecordId);
            _context.Update(loan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PayFineAsync(int loanRecordId)
        {
            var loan = await _context.LoanRecords
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loan == null || loan.IsFinePaid || loan.FineAmount <= 0) return false;

            loan.IsFinePaid = true;
            _context.Update(loan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}