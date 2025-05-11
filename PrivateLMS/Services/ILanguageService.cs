using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface ILanguageService
    {
        Task<List<LanguageViewModel>> GetAllLanguagesAsync();
        Task<LanguageViewModel?> GetLanguageDetailsAsync(int languageId);
        Task<bool> CreateLanguageAsync(LanguageViewModel model);
        Task<bool> UpdateLanguageAsync(int id, LanguageViewModel model);
        Task<bool> DeleteLanguageAsync(int id);
        Task<PagedResultViewModel<LanguageViewModel>> GetPagedLanguagesAsync(int page, int pageSize);
    }
}
