using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using PrivateLMS.Data;
using Microsoft.EntityFrameworkCore;

namespace PrivateLMS.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IBookService _bookService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(LibraryDbContext context, IBookService bookService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _bookService = bookService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync();
                return View(books);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the books: {ex.Message}";
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var book = await _bookService.GetBookDetailsAsync(id.Value);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return View("NotFound");
                }

                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book details: {ex.Message}";
                return View("Error");
            }
        }

        public IActionResult Loan(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loan.";
                return View("NotFound");
            }

            return RedirectToAction("Create", "Loan", new { bookId = id });
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            var viewModel = new BookViewModel
            {
                AvailablePublishers = _context.Publishers.ToList(),
                AvailableCategories = _context.Categories.ToList()
            };
            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? coverImagePath = null;
                    if (viewModel.CoverImage != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/book-covers");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.CoverImage.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.CoverImage.CopyToAsync(stream);
                        }
                        coverImagePath = $"/images/book-covers/{fileName}";
                    }

                    var book = new Book
                    {
                        Title = viewModel.Title,
                        Author = viewModel.Author,
                        ISBN = viewModel.ISBN,
                        Language = viewModel.Language,
                        PublishedDate = viewModel.PublishedDate,
                        IsAvailable = viewModel.IsAvailable,
                        CoverImagePath = coverImagePath,
                        PublisherId = viewModel.PublisherId,
                        BookCategories = viewModel.SelectedCategoryIds
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

                    TempData["SuccessMessage"] = $"Successfully added the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while adding the book: {ex.Message}";
                }
            }

            viewModel.AvailablePublishers = _context.Publishers.ToList();
            viewModel.AvailableCategories = _context.Categories.ToList();
            TempData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Please fix the errors and try again.";
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
                    CoverImagePath = book.CoverImagePath,
                    PublisherId = book.PublisherId,
                    SelectedCategoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList(),
                    AvailablePublishers = _context.Publishers.ToList(),
                    AvailableCategories = _context.Categories.ToList()
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

                    if (viewModel.CoverImage != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/book-covers");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.CoverImage.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.CoverImage.CopyToAsync(stream);
                        }
                        if (!string.IsNullOrEmpty(book.CoverImagePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, book.CoverImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        book.CoverImagePath = $"/images/book-covers/{fileName}";
                    }

                    book.Title = viewModel.Title;
                    book.Author = viewModel.Author;
                    book.ISBN = viewModel.ISBN;
                    book.Language = viewModel.Language;
                    book.PublishedDate = viewModel.PublishedDate;
                    book.IsAvailable = viewModel.IsAvailable;
                    book.PublisherId = viewModel.PublisherId;

                    book.BookCategories.Clear();
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
                }
            }

            viewModel.AvailablePublishers = _context.Publishers.ToList();
            viewModel.AvailableCategories = _context.Categories.ToList();
            TempData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Please fix the errors and try again.";
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
                    .Include(b => b.Publisher)
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
                    CoverImagePath = book.CoverImagePath,
                    PublisherId = book.PublisherId,
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