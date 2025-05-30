﻿using PrivateLMS.Models;
using PrivateLMS.ViewModels;

namespace PrivateLMS.Services
{
    public interface ILoanService
    {
        Task<List<LoanViewModel>> GetAllLoansAsync();
        Task<LoanViewModel?> GetLoanFormAsync(int bookId);
        Task<bool> CreateLoanAsync(LoanViewModel model);
        Task<ReturnViewModel?> GetReturnFormAsync(int loanRecordId);
        Task<bool> ReturnLoanAsync(int loanRecordId);
        Task<List<LoanViewModel>> GetUserLoansAsync(string? username);
        Task<bool> RenewLoanAsync(int loanRecordId);
        Task<List<LoanViewModel>> GetAllUserLoansAsync(string? username);
        Task<List<LoanViewModel>> GetOverdueLoansAsync(string? username);
        Task<List<LoanViewModel>> GetUserActiveLoansAsync(string? username);
        Task<int> GetActiveLoanCountAsync(int userId);
        Task<PagedResultViewModel<LoanViewModel>> GetPagedUserActiveLoansAsync(string userName, int page, int pageSize);
        Task<PagedResultViewModel<LoanViewModel>> GetPagedAllLoansAsync(int page, int pageSize);
        Task<PagedResultViewModel<LoanViewModel>> GetPagedAllUserLoansAsync(string userName, int page, int pageSize);
    }
}