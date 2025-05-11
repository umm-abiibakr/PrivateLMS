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
    public class LanguageService : ILanguageService
    {
        private readonly LibraryDbContext _context;

        public LanguageService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<LanguageViewModel>> GetAllLanguagesAsync()
        {
            return await _context.Languages
                .AsNoTracking()
                .Select(l => new LanguageViewModel
                {
                    LanguageId = l.LanguageId,
                    Name = l.Name,
                    BookCount = _context.Books.Count(b => b.LanguageId == l.LanguageId)
                })
                .ToListAsync();
        }

        public async Task<LanguageViewModel?> GetLanguageDetailsAsync(int languageId)
        {
            var language = await _context.Languages
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LanguageId == languageId);

            if (language == null)
                return null;

            var books = await _context.Books
                .Where(b => b.LanguageId == languageId)
                .Select(b => b.Title)
                .ToListAsync();

            return new LanguageViewModel
            {
                LanguageId = language.LanguageId,
                Name = language.Name,
                BookCount = books.Count,
                Books = books
            };
        }

        public async Task<bool> CreateLanguageAsync(LanguageViewModel model)
        {
            var language = new Language
            {
                Name = model.Name
            };

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLanguageAsync(int id, LanguageViewModel model)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
                return false;

            language.Name = model.Name;
            _context.Update(language);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLanguageAsync(int id)
        {
            var hasBooks = await _context.Books.AnyAsync(b => b.LanguageId == id);
            if (hasBooks)
                throw new InvalidOperationException("Cannot delete language with associated books.");

            var language = await _context.Languages.FindAsync(id);
            if (language == null)
                return false;

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResultViewModel<LanguageViewModel>> GetPagedLanguagesAsync(int page, int pageSize)
        {
            var query = _context.Languages.AsNoTracking();

            var totalItems = await query.CountAsync();

            var languages = await query
                .OrderBy(l => l.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LanguageViewModel
                {
                    LanguageId = l.LanguageId,
                    Name = l.Name,
                    BookCount = _context.Books.Count(b => b.LanguageId == l.LanguageId)
                })
                .ToListAsync();

            return new PagedResultViewModel<LanguageViewModel>
            {
                Items = languages,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }
    }
}
