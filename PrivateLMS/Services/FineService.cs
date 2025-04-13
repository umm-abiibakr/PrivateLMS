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
            return await _context.Fines
                .Include(f => f.LoanRecord)
                    .ThenInclude(lr => lr.Book)
                .Include(f => f.LoanRecord)
                    .ThenInclude(lr => lr.User)
                .AsNoTracking()
                .Select(f => new FineViewModel
                {
                    Id = f.Id,
                    LoanRecordId = f.LoanId,
                    BookTitle = f.LoanRecord.Book != null ? f.LoanRecord.Book.Title ?? "Unknown" : "Unknown",
                    LoanerName = f.LoanRecord.User != null ? $"{f.LoanRecord.User.FirstName} {f.LoanRecord.User.LastName}" : "Unknown",
                    LoanDate = f.LoanRecord.LoanDate,
                    DueDate = f.LoanRecord.DueDate,
                    ReturnDate = f.LoanRecord.ReturnDate,
                    IssuedDate = f.IssuedDate,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid
                })
                .ToListAsync();
        }

        public async Task<List<FineViewModel>> GetUserFinesAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<FineViewModel>();
            }

            return await _context.Fines
                .Include(f => f.LoanRecord)
                    .ThenInclude(lr => lr.Book)
                .Include(f => f.LoanRecord)
                    .ThenInclude(lr => lr.User)
                .Where(f => f.LoanRecord.User != null && f.LoanRecord.User.UserName == username)
                .AsNoTracking()
                .Select(f => new FineViewModel
                {
                    Id = f.Id,
                    LoanRecordId = f.LoanId,
                    BookTitle = f.LoanRecord.Book != null ? f.LoanRecord.Book.Title ?? "Unknown" : "Unknown",
                    LoanerName = f.LoanRecord.User != null ? $"{f.LoanRecord.User.FirstName} {f.LoanRecord.User.LastName}" : "Unknown",
                    LoanDate = f.LoanRecord.LoanDate,
                    DueDate = f.LoanRecord.DueDate,
                    ReturnDate = f.LoanRecord.ReturnDate,
                    IssuedDate = f.IssuedDate,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid
                })
                .ToListAsync();
        }

        public async Task<FineViewModel?> GetFineByIdAsync(int fineId)
        {
            return await _context.Fines
                .Include(f => f.LoanRecord)
                    .ThenInclude(lr => lr.Book)
                .Include(f => f.LoanRecord)
                    .ThenInclude(lr => lr.User)
                .AsNoTracking()
                .Where(f => f.Id == fineId)
                .Select(f => new FineViewModel
                {
                    Id = f.Id,
                    LoanRecordId = f.LoanId,
                    BookTitle = f.LoanRecord.Book != null ? f.LoanRecord.Book.Title ?? "Unknown" : "Unknown",
                    LoanerName = f.LoanRecord.User != null ? $"{f.LoanRecord.User.FirstName} {f.LoanRecord.User.LastName}" : "Unknown",
                    LoanDate = f.LoanRecord.LoanDate,
                    DueDate = f.LoanRecord.DueDate,
                    ReturnDate = f.LoanRecord.ReturnDate,
                    IssuedDate = f.IssuedDate,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid
                })
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> CalculateFineAsync(int loanRecordId)
        {
            var loan = await _context.LoanRecords
                .AsNoTracking()
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
                .Include(lr => lr.Fines)
                .FirstOrDefaultAsync(lr => lr.LoanRecordId == loanRecordId);

            if (loan == null || loan.ReturnDate == null) return false;

            var fineAmount = await CalculateFineAsync(loanRecordId);
            if (fineAmount <= 0) return true; // No fine to apply

            var existingFine = loan.Fines.FirstOrDefault();
            if (existingFine != null)
            {
                existingFine.Amount = fineAmount;
                existingFine.IsPaid = false;
                existingFine.IssuedDate = loan.ReturnDate.Value;
                _context.Update(existingFine);
            }
            else
            {
                var fine = new Fine
                {
                    UserId = loan.UserId,
                    LoanId = loanRecordId,
                    Amount = fineAmount,
                    IssuedDate = loan.ReturnDate.Value,
                    IsPaid = false
                };
                _context.Fines.Add(fine);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PayFineAsync(int fineId)
        {
            var fine = await _context.Fines
                .FirstOrDefaultAsync(f => f.Id == fineId);

            if (fine == null || fine.IsPaid || fine.Amount <= 0) return false;

            fine.IsPaid = true;
            _context.Update(fine);

            // Log the fine payment
            _context.UserActivities.Add(new UserActivity
            {
                UserId = fine.UserId,
                Action = "PayFine",
                Timestamp = DateTime.UtcNow,
                Details = $"User paid fine of NGN {fine.Amount} for loan ID {fine.LoanId}"
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}