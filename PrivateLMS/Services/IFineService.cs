using PrivateLMS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface IFineService
    {
        Task<List<FineViewModel>> GetAllFinesAsync();
        Task<List<FineViewModel>> GetUserFinesAsync(string? username);
        Task<decimal> CalculateFineAsync(int loanRecordId);
        Task<bool> UpdateFineAsync(int loanRecordId);
        Task<bool> PayFineAsync(int loanRecordId);
    }
}