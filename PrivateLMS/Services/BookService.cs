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

        public BookService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookViewModel>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    ISBN = b.ISBN,
                    Language = b.Language,
                    PublishedDate = b.PublishedDate,
                    Description = b.Description,
                    AvailableCopies = b.AvailableCopies,
                    IsAvailable = b.IsAvailable,
                    CoverImagePath = b.CoverImagePath,
                    PublisherId = b.PublisherId,
                    SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList(),
                    AvailableAuthors = _context.Authors.ToList(),
                    AvailablePublishers = _context.Publishers.ToList(),
                    AvailableCategories = _context.Categories.ToList()
                })
                .ToListAsync();
        }

        public async Task<BookViewModel?> GetBookDetailsAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null)
            {
                return null;
            }

            return new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.AuthorId,
                ISBN = book.ISBN,
                Language = book.Language,
                PublishedDate = book.PublishedDate,
                Description = book.Description,
                AvailableCopies = book.AvailableCopies,
                IsAvailable = book.IsAvailable,
                CoverImagePath = book.CoverImagePath,
                PublisherId = book.PublisherId,
                SelectedCategoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList(),
                AvailableAuthors = await _context.Authors.ToListAsync(),
                AvailablePublishers = await _context.Publishers.ToListAsync(),
                AvailableCategories = await _context.Categories.ToListAsync()
            };
        }

        public async Task<bool> CreateBookAsync(BookViewModel model, string? coverImagePath)
        {
            var book = new Book
            {
                Title = model.Title,
                AuthorId = model.AuthorId,
                ISBN = model.ISBN,
                Language = model.Language,
                PublishedDate = model.PublishedDate,
                Description = model.Description,
                AvailableCopies = model.AvailableCopies,
                IsAvailable = model.IsAvailable,
                CoverImagePath = coverImagePath,
                PublisherId = model.PublisherId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Add categories
            if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory { BookId = book.BookId, CategoryId = categoryId });
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpdateBookAsync(int id, BookViewModel model, string? coverImagePath)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return false;
            }

            book.Title = model.Title;
            book.AuthorId = model.AuthorId;
            book.ISBN = model.ISBN;
            book.Language = model.Language;
            book.PublishedDate = model.PublishedDate;
            book.Description = model.Description;
            book.AvailableCopies = model.AvailableCopies;
            book.IsAvailable = model.IsAvailable;
            book.PublisherId = model.PublisherId;
            if (!string.IsNullOrEmpty(coverImagePath))
            {
                book.CoverImagePath = coverImagePath;
            }

            // Update categories
            var existingCategories = book.BookCategories.ToList();
            _context.BookCategories.RemoveRange(existingCategories);

            if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory { BookId = book.BookId, CategoryId = categoryId });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            return await _context.Books.FindAsync(bookId);
        }

        public async Task<List<Publisher>> GetAllPublishersAsync()
        {
            return await _context.Publishers.ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return false;
            }

            _context.BookCategories.RemoveRange(book.BookCategories);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BookViewModel>> SearchBooksAsync(string? searchTerm)
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) || (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }

            return await query.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                AuthorId = b.AuthorId,
                ISBN = b.ISBN,
                Language = b.Language,
                PublishedDate = b.PublishedDate,
                Description = b.Description,
                AvailableCopies = b.AvailableCopies,
                IsAvailable = b.IsAvailable,
                CoverImagePath = b.CoverImagePath,
                PublisherId = b.PublisherId,
                SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList(),
                AvailableAuthors = _context.Authors.ToList(),
                AvailablePublishers = _context.Publishers.ToList(),
                AvailableCategories = _context.Categories.ToList()
            }).ToListAsync();
        }

        public async Task<List<BookViewModel>> SearchBooksByCategoryAsync(string? searchTerm, int? categoryId)
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) || (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));
            }

            return await query.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                AuthorId = b.AuthorId,
                ISBN = b.ISBN,
                Language = b.Language,
                PublishedDate = b.PublishedDate,
                Description = b.Description,
                AvailableCopies = b.AvailableCopies,
                IsAvailable = b.IsAvailable,
                CoverImagePath = b.CoverImagePath,
                PublisherId = b.PublisherId,
                SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList(),
                AvailableAuthors = _context.Authors.ToList(),
                AvailablePublishers = _context.Publishers.ToList(),
                AvailableCategories = _context.Categories.ToList()
            }).ToListAsync();
        }

        public async Task<List<BookViewModel>> GetRecommendedBooksAsync(int userId)
        {
            // Simple recommendation: Books that the user hasn't loaned yet
            var userLoans = await _context.LoanRecords
                .Where(lr => lr.UserId == userId)
                .Select(lr => lr.BookId)
                .ToListAsync();

            return await _context.Books
                .Where(b => !userLoans.Contains(b.BookId))
                .OrderBy(b => Guid.NewGuid()) // Randomize for now
                .Take(5)
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    CoverImagePath = b.CoverImagePath,
                    IsAvailable = b.IsAvailable,
                    AvailableCopies = b.AvailableCopies
                })
                .ToListAsync();
        }

        public async Task<List<BookViewModel>> GetNewBooksAsync(int count)
        {
            return await _context.Books
                .OrderByDescending(b => b.BookId)
                .Take(count)
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    CoverImagePath = b.CoverImagePath,
                    IsAvailable = b.IsAvailable,
                    AvailableCopies = b.AvailableCopies
                })
                .ToListAsync();
        }

        public async Task<List<BookViewModel>> GetPopularBooksAsync(int count)
        {
            return await _context.Books
                .Select(b => new
                {
                    Book = b,
                    LoanCount = _context.LoanRecords.Count(lr => lr.BookId == b.BookId)
                })
                .OrderByDescending(x => x.LoanCount)
                .ThenByDescending(x => x.Book.AvailableCopies)
                .Take(count)
                .Select(x => new BookViewModel
                {
                    BookId = x.Book.BookId,
                    Title = x.Book.Title,
                    Description = x.Book.Description,
                    CoverImagePath = x.Book.CoverImagePath,
                    IsAvailable = x.Book.IsAvailable,
                    AvailableCopies = x.Book.AvailableCopies
                })
                .ToListAsync();
        }
    }
}