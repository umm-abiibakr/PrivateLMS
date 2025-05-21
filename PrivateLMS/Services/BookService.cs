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
                .Include(b => b.Language)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    ISBN = b.ISBN,
                    LanguageId = b.LanguageId,
                    PublishedDate = b.PublishedDate,
                    Description = b.Description,
                    AvailableCopies = b.AvailableCopies,
                    IsAvailable = b.IsAvailable,
                    CoverImagePath = b.CoverImagePath,
                    PublisherId = b.PublisherId,
                    SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList(),
                    AvailableAuthors = _context.Authors.ToList(),
                    AvailablePublishers = _context.Publishers.ToList(),
                    AvailableCategories = _context.Categories.ToList(),
                    AvailableLanguages = _context.Languages.ToList()
                }).ToListAsync();
        }

        public async Task<BookViewModel?> GetBookDetailsAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null) return null;

            var viewModel = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.AuthorId,
                ISBN = book.ISBN,
                LanguageId = book.LanguageId,
                PublishedDate = book.PublishedDate,
                Description = book.Description,
                AvailableCopies = book.AvailableCopies,
                IsAvailable = book.IsAvailable,
                CoverImagePath = book.CoverImagePath,
                PublisherId = book.PublisherId,
                SelectedCategoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList(),
                AvailableAuthors = await _context.Authors.ToListAsync(),
                AvailablePublishers = await _context.Publishers.ToListAsync(),
                AvailableCategories = await _context.Categories.ToListAsync(),
                AvailableLanguages = await _context.Languages.ToListAsync(),
                AverageRating = await _context.BookRatings
                    .Where(br => br.BookId == bookId)
                    .AnyAsync()
                    ? (float)await _context.BookRatings
                        .Where(br => br.BookId == bookId)
                        .AverageAsync(br => br.Rating)
                    : 0f,
                RatingCount = await _context.BookRatings
                    .CountAsync(br => br.BookId == bookId)
            };

            // Fetch reviews
            viewModel.Reviews = await _context.BookRatings
                .Where(br => br.BookId == bookId)
                .Include(br => br.User) // Include ApplicationUser for user details
                .Select(br => new BookReviewViewModel
                {
                    Rating = br.Rating,
                    Review = br.Review ?? string.Empty,
                    UserName = br.User != null ? br.User.UserName : "Anonymous",
                    RatedOn = br.RatedOn
                })
                .OrderByDescending(br => br.RatedOn)
                .ToListAsync();

            return viewModel;
        }

        public async Task<bool> CreateBookAsync(BookViewModel model, string? coverImagePath)
        {
            var book = new Book
            {
                Title = model.Title,
                AuthorId = model.AuthorId,
                ISBN = model.ISBN,
                LanguageId = model.LanguageId,
                PublishedDate = model.PublishedDate,
                Description = model.Description,
                AvailableCopies = model.AvailableCopies,
                IsAvailable = model.IsAvailable,
                CoverImagePath = coverImagePath,
                PublisherId = model.PublisherId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (model.SelectedCategoryIds?.Any() == true)
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

            if (book == null) return false;

            book.Title = model.Title;
            book.AuthorId = model.AuthorId;
            book.ISBN = model.ISBN;
            book.LanguageId = model.LanguageId;
            book.PublishedDate = model.PublishedDate;
            book.Description = model.Description;
            book.AvailableCopies = model.AvailableCopies;
            book.IsAvailable = model.IsAvailable;
            book.PublisherId = model.PublisherId;

            if (!string.IsNullOrEmpty(coverImagePath))
                book.CoverImagePath = coverImagePath;

            _context.BookCategories.RemoveRange(book.BookCategories);

            if (model.SelectedCategoryIds?.Any() == true)
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory { BookId = book.BookId, CategoryId = categoryId });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return false;

            _context.BookCategories.RemoveRange(book.BookCategories);
            _context.Books.Remove(book);
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

        public async Task<List<Language>> GetAllLanguagesAsync()
        {
            return await _context.Languages.ToListAsync();
        }

        public async Task<List<BookViewModel>> SearchBooksAsync(string? searchTerm)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchTerm) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }

            return await query.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                AuthorId = b.AuthorId,
                ISBN = b.ISBN,
                LanguageId = b.LanguageId,
                PublishedDate = b.PublishedDate,
                Description = b.Description,
                AvailableCopies = b.AvailableCopies,
                IsAvailable = b.IsAvailable,
                CoverImagePath = b.CoverImagePath,
                PublisherId = b.PublisherId,
                SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList(),
                AvailableAuthors = _context.Authors.ToList(),
                AvailablePublishers = _context.Publishers.ToList(),
                AvailableCategories = _context.Categories.ToList(),
                AvailableLanguages = _context.Languages.ToList()
            }).ToListAsync();
        }

        public async Task<List<BookViewModel>> SearchBooksByCategoryAsync(string? searchTerm, int? categoryId)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchTerm) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId));
            }

            return await query.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                AuthorId = b.AuthorId,
                ISBN = b.ISBN,
                LanguageId = b.LanguageId,
                PublishedDate = b.PublishedDate,
                Description = b.Description,
                AvailableCopies = b.AvailableCopies,
                IsAvailable = b.IsAvailable,
                CoverImagePath = b.CoverImagePath,
                PublisherId = b.PublisherId,
                SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList(),
                AvailableAuthors = _context.Authors.ToList(),
                AvailablePublishers = _context.Publishers.ToList(),
                AvailableCategories = _context.Categories.ToList(),
                AvailableLanguages = _context.Languages.ToList()
            }).ToListAsync();
        }

        public async Task<List<BookRecommendationViewModel>> GetRecommendedBooksAsync(int userId)
        {
            // Fetch user preferences
            var userCategoryPrefs = await _context.CategoryPreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.CategoryId)
                .ToListAsync();

            var userAuthorPrefs = await _context.AuthorPreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.AuthorId)
                .ToListAsync();

            var userLanguagePrefs = await _context.LanguagePreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.LanguageId)
                .ToListAsync();

            // Get all books with related data
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Language)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .ToListAsync();

            var recommendations = new List<BookRecommendationViewModel>();

            foreach (var book in books)
            {
                // Preference scoring
                float categoryMatch = book.BookCategories.Any(bc => userCategoryPrefs.Contains(bc.CategoryId)) ? 1f : 0f;
                float authorMatch = userAuthorPrefs.Contains(book.AuthorId) ? 1f : 0f;
                float languageMatch = userLanguagePrefs.Contains(book.LanguageId) ? 1f : 0f;

                float score = (categoryMatch + authorMatch + languageMatch) / 3f;

                if (score > 0f)
                {
                    recommendations.Add(new BookRecommendationViewModel
                    {
                        Book = book,
                        RecommendationScore = score
                    });
                }
            }

            return recommendations
                .OrderByDescending(r => r.RecommendationScore)
                .Take(5)
                .ToList();
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
                }).ToListAsync();
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
                }).ToListAsync();
        }

        public async Task<PagedResultViewModel<BookViewModel>> GetPagedBooksAsync(string? searchTerm, int? categoryId, int? authorId, int page, int pageSize)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) ||
                                         (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));
            }

            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            var totalItems = await query.CountAsync();
            var books = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    AuthorId = b.AuthorId,
                    PublisherId = b.PublisherId,
                    ISBN = b.ISBN,
                    LanguageId = b.LanguageId,
                    PublishedDate = b.PublishedDate,
                    AvailableCopies = b.AvailableCopies,
                    IsAvailable = b.IsAvailable,
                    CoverImagePath = b.CoverImagePath,
                    SelectedCategoryIds = b.BookCategories.Select(bc => bc.CategoryId).ToList()
                }).ToListAsync();

            var authors = await _context.Authors.ToListAsync();
            var publishers = await _context.Publishers.ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var languages = await _context.Languages.ToListAsync();

            foreach (var book in books)
            {
                book.AvailableAuthors = authors;
                book.AvailablePublishers = publishers;
                book.AvailableCategories = categories;
                book.AvailableLanguages = languages;
            }

            return new PagedResultViewModel<BookViewModel>
            {
                Items = books,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }
    }
}
