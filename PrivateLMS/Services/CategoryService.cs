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
    public class CategoryService : ICategoryService
    {
        private readonly LibraryDbContext _context;

        public CategoryService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.BookCategories)
                .AsNoTracking()
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    BookCount = c.BookCategories.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryViewModel?> GetCategoryDetailsAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.BookCategories)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return null;
            }

            return new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                BookCount = category.BookCategories.Count
            };
        }

        public async Task<bool> CreateCategoryAsync(CategoryViewModel model)
        {
            var category = new Category
            {
                CategoryName = model.CategoryName
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(int id, CategoryViewModel model)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            category.CategoryName = model.CategoryName;
            _context.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.BookCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return false;
            }

            if (category.BookCategories.Any())
            {
                throw new InvalidOperationException("Cannot delete category with associated books.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResultViewModel<CategoryViewModel>> GetPagedCategoriesAsync(int page, int pageSize)
        {
            var query = _context.Categories
                .Include(c => c.BookCategories)
                .AsNoTracking();

            var totalItems = await query.CountAsync();
            var categories = await query
                .OrderBy(c => c.CategoryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    BookCount = c.BookCategories.Count
                })
                .ToListAsync();

            return new PagedResultViewModel<CategoryViewModel>
            {
                Items = categories,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }
    }
}