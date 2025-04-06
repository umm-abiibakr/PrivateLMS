using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
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
                .AsNoTracking()
                .Select(b => new BookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    CoverImagePath = b.CoverImagePath,
                    IsAvailable = b.IsAvailable
                })
                .ToListAsync();
        }

        public async Task<BookViewModel> GetBookDetailsAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.Publisher)
                .Include(b => b.LoanRecords)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null)
            {
                return null;
            }

            return new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Language = book.Language,
                PublishedDate = book.PublishedDate,
                IsAvailable = book.IsAvailable,
                CoverImagePath = book.CoverImagePath,
                PublisherId = book.PublisherId,
                AvailablePublishers = _context.Publishers.ToList(),
                AvailableCategories = book.BookCategories.Select(bc => bc.Category).ToList(),
                LoanRecords = book.LoanRecords?.ToList() ?? new List<LoanRecord>()
            };
        }

        public async Task<bool> CreateBookAsync(BookViewModel model, string coverImagePath)
        {
            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                ISBN = model.ISBN,
                Language = model.Language,
                PublishedDate = model.PublishedDate,
                IsAvailable = model.IsAvailable,
                CoverImagePath = coverImagePath,
                PublisherId = model.PublisherId,
                BookCategories = model.SelectedCategoryIds
                    .Select(categoryId => new BookCategory { CategoryId = categoryId })
                    .ToList()
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

        public async Task<bool> UpdateBookAsync(int id, BookViewModel model, string coverImagePath)
        {
            var book = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return false;
            }

            book.Title = model.Title;
            book.Author = model.Author;
            book.ISBN = model.ISBN;
            book.Language = model.Language;
            book.PublishedDate = model.PublishedDate;
            book.IsAvailable = model.IsAvailable;
            book.PublisherId = model.PublisherId;
            if (!string.IsNullOrEmpty(coverImagePath))
            {
                book.CoverImagePath = coverImagePath;
            }

            book.BookCategories.Clear();
            book.BookCategories = model.SelectedCategoryIds
                .Select(categoryId => new BookCategory { BookId = book.BookId, CategoryId = categoryId })
                .ToList();

            _context.Update(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Book> GetBookByIdAsync(int bookId)
        {
            return await _context.Books.FindAsync(bookId);
        }
    }
}