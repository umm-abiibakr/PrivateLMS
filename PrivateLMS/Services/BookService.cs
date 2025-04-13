using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public class BookService : IBookService
    {
        private readonly LibraryDbContext _context;
        private readonly IBookRatingService _bookRatingService;
        private readonly List<(float BorrowFrequency, float CategoryAffinity, float BookPopularity, float RecommendationScore)> _lookupTableData;
        private bool _isLookupTableLoaded;

        public BookService(LibraryDbContext context, IBookRatingService bookRatingService)
        {
            _context = context;
            _bookRatingService = bookRatingService;
            _isLookupTableLoaded = false;
            _lookupTableData = new List<(float BorrowFrequency, float CategoryAffinity, float BookPopularity, float RecommendationScore)>();

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine($"Base directory: {baseDir}");

                var path = Path.Combine(baseDir, "lookup_table.csv");
                Console.WriteLine($"Attempting to load lookup_table.csv from: {path}");

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"lookup_table.csv not found at path: {path}");
                }

                var lines = File.ReadAllLines(path).Skip(1); // Skip header
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length != 4) continue; // Skip malformed lines
                    if (float.TryParse(parts[0], out float bf) &&
                        float.TryParse(parts[1], out float ca) &&
                        float.TryParse(parts[2], out float bp) &&
                        float.TryParse(parts[3], out float score))
                    {
                        _lookupTableData.Add((bf, ca, bp, score));
                    }
                }

                if (_lookupTableData.Count == 0)
                {
                    throw new InvalidOperationException("Loaded lookup table is empty.");
                }

                _isLookupTableLoaded = true;
                Console.WriteLine($"Successfully loaded lookup_table.csv with {_lookupTableData.Count} entries");
                Console.WriteLine($"First entry: BorrowFrequency={_lookupTableData[0].BorrowFrequency}, " +
                                 $"CategoryAffinity={_lookupTableData[0].CategoryAffinity}, " +
                                 $"BookPopularity={_lookupTableData[0].BookPopularity}, " +
                                 $"Score={_lookupTableData[0].RecommendationScore}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading lookup table: {ex.Message}\nStack Trace: {ex.StackTrace}");
                _lookupTableData.Clear();
                _isLookupTableLoaded = false;
            }
        }

        public async Task<List<BookViewModel>> GetRecommendedBooksAsync(int userId)
        {
            if (!_isLookupTableLoaded || _lookupTableData.Count == 0)
            {
                Console.WriteLine("Warning: Lookup table not loaded. Returning empty recommendations.");
                return new List<BookViewModel>();
            }

            var userLoans = await _context.LoanRecords
                .Where(lr => lr.UserId == userId)
                .Select(lr => new { lr.BookId, lr.Book })
                .ToListAsync();

            float borrowFrequency = Math.Min(userLoans.Count, 150f);

            var books = await _context.Books
                .Include(b => b.BookCategories)
                .Where(b => b.IsAvailable && b.AvailableCopies > 0)
                .ToListAsync();

            var recommendations = new List<(Book Book, float Score)>();

            foreach (var book in books)
            {
                var bookCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>();
                var categoryLoans = userLoans.Count(l => l.Book.BookCategories?.Any(bc => bookCategoryIds.Contains(bc.CategoryId)) ?? false);
                float categoryAffinity = Math.Min(categoryLoans, 40f);

                var bookLoans = await _context.LoanRecords.CountAsync(lr => lr.BookId == book.BookId);
                float bookPopularity = Math.Min(bookLoans + book.AvailableCopies, 400f);

                float score = GetLookupScore(borrowFrequency, categoryAffinity, bookPopularity);
                recommendations.Add((book, score));
            }

            var result = recommendations
                .OrderByDescending(r => r.Score)
                .Take(6)
                .Select(r => new BookViewModel
                {
                    BookId = r.Book.BookId,
                    Title = r.Book.Title,
                    Description = r.Book.Description,
                    AvailableCopies = r.Book.AvailableCopies,
                    IsAvailable = r.Book.IsAvailable,
                    CoverImagePath = r.Book.CoverImagePath,
                    AverageRating = Task.Run(() => _bookRatingService.GetAverageRatingAsync(r.Book.BookId)).Result,
                    RatingCount = Task.Run(() => _bookRatingService.GetRatingCountAsync(r.Book.BookId)).Result
                })
                .ToList();

            return result;
        }

        private float GetLookupScore(float bf, float ca, float bp)
        {
            if (!_isLookupTableLoaded || _lookupTableData.Count == 0)
            {
                Console.WriteLine("Error: Cannot compute score because lookup table is not loaded.");
                return 0f;
            }

            float minDist = float.MaxValue;
            float score = 0f;

            try
            {
                foreach (var entry in _lookupTableData)
                {
                    float table_bf = entry.BorrowFrequency;
                    float table_ca = entry.CategoryAffinity;
                    float table_bp = entry.BookPopularity;
                    float table_score = entry.RecommendationScore;

                    float dist = (float)Math.Sqrt(
                        Math.Pow(bf - table_bf, 2) +
                        Math.Pow(ca - table_ca, 2) +
                        Math.Pow(bp - table_bp, 2));

                    if (dist < minDist)
                    {
                        minDist = dist;
                        score = table_score;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLookupScore: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return 0f;
            }

            return score;
        }

        public async Task<List<BookViewModel>> GetAllBooksAsync()
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .AsNoTracking()
                .ToListAsync();

            var bookViewModels = new List<BookViewModel>();
            foreach (var book in books)
            {
                var bookViewModel = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Description = book.Description,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    AvailableCopies = book.AvailableCopies,
                    IsAvailable = book.IsAvailable,
                    CoverImagePath = book.CoverImagePath,
                    AuthorId = book.AuthorId,
                    PublisherId = book.PublisherId,
                    SelectedCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>(),
                    AverageRating = await _bookRatingService.GetAverageRatingAsync(book.BookId),
                    RatingCount = await _bookRatingService.GetRatingCountAsync(book.BookId)
                };
                bookViewModels.Add(bookViewModel);
            }

            return bookViewModels;
        }

        public async Task<BookViewModel?> GetBookDetailsAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null) return null;

            var bookViewModel = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                ISBN = book.ISBN,
                Language = book.Language,
                PublishedDate = book.PublishedDate,
                AvailableCopies = book.AvailableCopies,
                IsAvailable = book.IsAvailable,
                CoverImagePath = book.CoverImagePath,
                AuthorId = book.AuthorId,
                PublisherId = book.PublisherId,
                SelectedCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>(),
                AvailableAuthors = await _context.Authors.ToListAsync(),
                AvailablePublishers = await _context.Publishers.ToListAsync(),
                AvailableCategories = await _context.Categories.ToListAsync(),
                AverageRating = await _bookRatingService.GetAverageRatingAsync(book.BookId),
                RatingCount = await _bookRatingService.GetRatingCountAsync(book.BookId)
            };

            return bookViewModel;
        }

        public async Task<bool> CreateBookAsync(BookViewModel model, string? coverImagePath)
        {
            var book = new Book
            {
                Title = model.Title,
                Description = model.Description,
                ISBN = model.ISBN,
                Language = model.Language,
                PublishedDate = model.PublishedDate,
                AvailableCopies = model.AvailableCopies,
                IsAvailable = model.IsAvailable,
                CoverImagePath = coverImagePath,
                AuthorId = model.AuthorId,
                PublisherId = model.PublisherId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (model.SelectedCategoryIds.Any())
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory
                    {
                        BookId = book.BookId,
                        CategoryId = categoryId
                    });
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
            book.Description = model.Description;
            book.ISBN = model.ISBN;
            book.Language = model.Language;
            book.PublishedDate = model.PublishedDate;
            book.AvailableCopies = model.AvailableCopies;
            book.IsAvailable = model.IsAvailable;
            if (coverImagePath != null) book.CoverImagePath = coverImagePath;
            book.AuthorId = model.AuthorId;
            book.PublisherId = model.PublisherId;

            var existingCategories = book.BookCategories.ToList();
            _context.BookCategories.RemoveRange(existingCategories);

            if (model.SelectedCategoryIds.Any())
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    _context.BookCategories.Add(new BookCategory
                    {
                        BookId = book.BookId,
                        CategoryId = categoryId
                    });
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
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BookViewModel>> SearchBooksAsync(string? searchTerm)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.Title.Contains(searchTerm) || b.Description.Contains(searchTerm));
            }

            var books = await query.ToListAsync();
            var bookViewModels = new List<BookViewModel>();

            foreach (var book in books)
            {
                var bookViewModel = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Description = book.Description,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    AvailableCopies = book.AvailableCopies,
                    IsAvailable = book.IsAvailable,
                    CoverImagePath = book.CoverImagePath,
                    AuthorId = book.AuthorId,
                    PublisherId = book.PublisherId,
                    SelectedCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>(),
                    AverageRating = await _bookRatingService.GetAverageRatingAsync(book.BookId),
                    RatingCount = await _bookRatingService.GetRatingCountAsync(book.BookId)
                };
                bookViewModels.Add(bookViewModel);
            }

            return bookViewModels;
        }

        public async Task<List<BookViewModel>> SearchBooksByCategoryAsync(string? searchTerm, int? categoryId)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId.Value));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.Title.Contains(searchTerm) || b.Description.Contains(searchTerm));
            }

            var books = await query.ToListAsync();
            var bookViewModels = new List<BookViewModel>();

            foreach (var book in books)
            {
                var bookViewModel = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Description = book.Description,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    AvailableCopies = book.AvailableCopies,
                    IsAvailable = book.IsAvailable,
                    CoverImagePath = book.CoverImagePath,
                    AuthorId = book.AuthorId,
                    PublisherId = book.PublisherId,
                    SelectedCategoryIds = book.BookCategories?.Select(bc => bc.CategoryId).ToList() ?? new List<int>(),
                    AverageRating = await _bookRatingService.GetAverageRatingAsync(book.BookId),
                    RatingCount = await _bookRatingService.GetRatingCountAsync(book.BookId)
                };
                bookViewModels.Add(bookViewModel);
            }

            return bookViewModels;
        }
    }
}