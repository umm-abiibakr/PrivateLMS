using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface IFineService
    {
        Task<List<FineViewModel>> GetAllFinesAsync();
        Task<List<FineViewModel>> GetUserFinesAsync(string? username);
        Task<FineViewModel?> GetFineByIdAsync(int fineId);
        Task<decimal> CalculateFineAsync(int loanRecordId);
        Task<bool> UpdateFineAsync(int loanRecordId);
        Task<bool> PayFineAsync(int fineId);
        Task<PagedResultViewModel<FineViewModel>> GetPagedUserFinesAsync(string userName, int page, int pageSize, bool unpaidOnly = false);
        Task<PagedResultViewModel<FineViewModel>> GetPagedAllFinesAsync(int page, int pageSize);
    }
}