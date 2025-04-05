using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;

namespace PrivateLMS.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _context.Books
                    .Include(b => b.BookCategories)
                        .ThenInclude(bc => bc.Category)
                    .Include(b => b.LoanRecords)
                    .AsNoTracking()
                    .ToListAsync();

                var bookViewModels = books.Select(book => new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    IsAvailable = book.IsAvailable,
                    AvailableCategories = book.BookCategories.Select(bc => bc.Category).ToList(),
                    LoanRecords = book.LoanRecords.ToList()
                }).ToList();

                return View(bookViewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the books: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books
                    .Include(b => b.BookCategories)
                        .ThenInclude(bc => bc.Category)
                    .Include(b => b.LoanRecords)
                    .FirstOrDefaultAsync(m => m.BookId == id);

                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return View("NotFound");
                }

                var viewModel = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    IsAvailable = book.IsAvailable,
                    AvailableCategories = book.BookCategories.Select(bc => bc.Category).ToList(),
                    LoanRecords = book.LoanRecords.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book details: {ex.Message}";
                return View("Error");
            }
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            var viewModel = new BookViewModel
            {
                AvailableCategories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var book = new Book
                    {
                        Title = viewModel.Title,
                        Author = viewModel.Author,
                        ISBN = viewModel.ISBN,
                        Language = viewModel.Language,
                        PublishedDate = viewModel.PublishedDate,
                        IsAvailable = viewModel.IsAvailable,
                        BookCategories = viewModel.SelectedCategoryIds
                            .Select(categoryId => new BookCategory { CategoryId = categoryId })
                            .ToList()
                    };

                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully added the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while adding the book: {ex.Message}";
                    return View(viewModel);
                }
            }

            viewModel.AvailableCategories = _context.Categories.ToList();
            TempData["ErrorMessage"] = "Please fix the errors and try again.";
            return View(viewModel);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books
                    .Include(b => b.BookCategories)
                    .FirstOrDefaultAsync(b => b.BookId == id);

                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return View("NotFound");
                }

                var viewModel = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    IsAvailable = book.IsAvailable,
                    AvailableCategories = _context.Categories.ToList(),
                    SelectedCategoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book for editing: {ex.Message}";
                return View("Error");
            }
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.BookId)
            {
                TempData["ErrorMessage"] = "Book ID mismatch.";
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var book = await _context.Books
                        .Include(b => b.BookCategories)
                        .FirstOrDefaultAsync(b => b.BookId == id);

                    if (book == null)
                    {
                        TempData["ErrorMessage"] = $"No book found with ID {id}.";
                        return View("NotFound");
                    }

                    // Update book properties
                    book.Title = viewModel.Title;
                    book.Author = viewModel.Author;
                    book.ISBN = viewModel.ISBN;
                    book.Language = viewModel.Language;
                    book.PublishedDate = viewModel.PublishedDate;
                    book.IsAvailable = viewModel.IsAvailable;

                    // Update categories
                    book.BookCategories.Clear(); // Remove existing categories
                    book.BookCategories = viewModel.SelectedCategoryIds
                        .Select(categoryId => new BookCategory { BookId = book.BookId, CategoryId = categoryId })
                        .ToList();

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully updated the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the book: {ex.Message}";
                    return View(viewModel);
                }
            }

            viewModel.AvailableCategories = _context.Categories.ToList();
            TempData["ErrorMessage"] = "Please fix the errors and try again.";
            return View(viewModel);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for deletion.";
                return View("NotFound");
            }

            try
            {
                var book = await _context.Books
                    .Include(b => b.BookCategories)
                        .ThenInclude(bc => bc.Category)
                    .FirstOrDefaultAsync(m => m.BookId == id);

                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return View("NotFound");
                }

                var viewModel = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    PublishedDate = book.PublishedDate,
                    IsAvailable = book.IsAvailable,
                    AvailableCategories = book.BookCategories.Select(bc => bc.Category).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book for deletion: {ex.Message}";
                return View("Error");
            }
        }
    }
}