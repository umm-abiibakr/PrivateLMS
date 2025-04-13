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
    public class BookService : IBookService
    {
        private readonly LibraryDbContext _context;
        private readonly IAuthorService _authorService;

        public BookService(LibraryDbContext context, IAuthorService authorService)
        {
            _context = context;
            _authorService = authorService;
        }

        public async Task<List<BookViewModel>> GetAllBooksAsync()
        {
            return await _context.Books
                .AsNoTracking()
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title ?? string.Empty,
                    CoverImagePath = b.CoverImagePath,
                    IsAvailable = b.IsAvailable,
                    AvailableCopies = b.AvailableCopies,
                    Description = b.Description
                })
                .ToListAsync();
        }

        public async Task<BookViewModel?> GetBookDetailsAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Publisher)
                .Include(b => b.Author)
                .Include(b => b.LoanRecords)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null) return null;

            return new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title ?? string.Empty,
                AuthorId = book.AuthorId,
                ISBN = book.ISBN ?? string.Empty,
                Language = book.Language ?? string.Empty,
                PublishedDate = book.PublishedDate,
                IsAvailable = book.IsAvailable,
                CoverImagePath = book.CoverImagePath,
                PublisherId = book.PublisherId,
                Description = book.Description,
                AvailableCopies = book.AvailableCopies,
                AvailableAuthors = await _authorService.GetAllAuthorsAsync(),
                AvailablePublishers = await GetAllPublishersAsync(),
                AvailableCategories = book.BookCategories?.Select(bc => bc.Category).ToList() ?? new List<Category>(),
                LoanRecords = book.LoanRecords?.ToList() ?? new List<LoanRecord>(),
                SelectedCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>()
            };
        }

        public async Task<bool> CreateBookAsync(BookViewModel model, string? coverImagePath)
        {
            var book = new Book
            {
                Title = model.Title ?? string.Empty,
                AuthorId = model.AuthorId,
                ISBN = model.ISBN ?? string.Empty,
                Language = model.Language ?? string.Empty,
                PublishedDate = model.PublishedDate,
                IsAvailable = model.IsAvailable,
                CoverImagePath = coverImagePath,
                PublisherId = model.PublisherId,
                Description = model.Description,
                AvailableCopies = model.AvailableCopies,
                BookCategories = model.SelectedCategoryIds?.Select(categoryId => new BookCategory { CategoryId = categoryId }).ToList() ?? new List<BookCategory>()
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            foreach (var bc in book.BookCategories)
            {
                bc.BookId = book.BookId;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBookAsync(int id, BookViewModel model, string? coverImagePath)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return false;

            book.Title = model.Title ?? string.Empty;
            book.AuthorId = model.AuthorId;
            book.ISBN = model.ISBN ?? string.Empty;
            book.Language = model.Language ?? string.Empty;
            book.PublishedDate = model.PublishedDate;
            book.IsAvailable = model.IsAvailable;
            book.PublisherId = model.PublisherId;
            book.Description = model.Description;
            book.AvailableCopies = model.AvailableCopies;
            if (!string.IsNullOrEmpty(coverImagePath))
            {
                book.CoverImagePath = coverImagePath;
            }

            book.BookCategories?.Clear();
            book.BookCategories = model.SelectedCategoryIds
                ?.Select(categoryId => new BookCategory { BookId = book.BookId, CategoryId = categoryId })
                .ToList() ?? new List<BookCategory>();

            _context.Update(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            return await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookId == bookId);
        }

        public async Task<List<Publisher>> GetAllPublishersAsync()
        {
            return await _context.Publishers
                .AsNoTracking()
                .ToListAsync() ?? new List<Publisher>();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .ToListAsync() ?? new List<Category>();
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BookViewModel>> SearchBooksAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllBooksAsync();
                }

                searchTerm = searchTerm.Trim();
                return await _context.Books
                    .AsNoTracking()
                    .Include(b => b.Author)
                    .Where(b => (b.Title != null && EF.Functions.Like(b.Title, $"%{searchTerm}%")) ||
                                (b.Author != null && EF.Functions.Like(b.Author.Name, $"%{searchTerm}%")))
                    .Select(b => new BookViewModel
                    {
                        BookId = b.BookId,
                        Title = b.Title ?? string.Empty,
                        CoverImagePath = b.CoverImagePath ?? string.Empty,
                        IsAvailable = b.IsAvailable,
                        AvailableCopies = b.AvailableCopies,
                        Description = b.Description,
                        AuthorId = b.AuthorId,
                        AvailableAuthors = new List<Author> { b.Author } // Single author for search result
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching books with term '{searchTerm}': {ex.Message}", ex);
            }
        }

        public async Task<List<BookViewModel>> GetRecommendedBooksAsync(int userId)
        {
            // Example: Recommend books from categories of user's past loans
            var userLoans = await _context.LoanRecords
                .Where(lr => lr.UserId == userId)
                .Select(lr => lr.BookId)
                .ToListAsync();

            var categories = await _context.BookCategories
                .Where(bc => userLoans.Contains(bc.BookId))
                .Select(bc => bc.CategoryId)
                .Distinct()
                .ToListAsync();

            var recommendedBooks = await _context.Books
                .Include(b => b.BookCategories)
                .Where(b => b.BookCategories.Any(bc => categories.Contains(bc.CategoryId)))
                .Where(b => b.IsAvailable && b.AvailableCopies > 0)
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    AvailableCopies = b.AvailableCopies,
                    IsAvailable = b.IsAvailable,
                    CoverImagePath = b.CoverImagePath
                })
                .Take(6) // Limit to 6 recommendations
                .ToListAsync();

            return recommendedBooks;
        }
    }
}