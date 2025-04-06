using PrivateLMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<CategoryViewModel> GetCategoryDetailsAsync(int categoryId);
        Task<bool> CreateCategoryAsync(CategoryViewModel model);
        Task<bool> UpdateCategoryAsync(int id, CategoryViewModel model);
        Task<bool> DeleteCategoryAsync(int id);
    }
}