using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using PrivateLMS.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace PrivateLMS.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService; // Added
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(IBookService bookService, IAuthorService authorService, IWebHostEnvironment webHostEnvironment)
        {
            _bookService = bookService;
            _authorService = authorService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            try
            {
                var books = await _bookService.SearchBooksAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View(books ?? new List<BookViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while searching books: {ex.Message}";
                return RedirectToAction("Error", "Home");
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
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")] // Re-added as per your preference
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new BookViewModel
                {
                    AvailableAuthors = await _authorService.GetAllAuthorsAsync(), // Use IAuthorService
                    AvailablePublishers = await _bookService.GetAllPublishersAsync(),
                    AvailableCategories = await _bookService.GetAllCategoriesAsync()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the create form: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

                    var success = await _bookService.CreateBookAsync(viewModel, coverImagePath);
                    if (!success)
                    {
                        TempData["ErrorMessage"] = "Failed to create the book.";
                        viewModel.AvailableAuthors = await _authorService.GetAllAuthorsAsync();
                        viewModel.AvailablePublishers = await _bookService.GetAllPublishersAsync();
                        viewModel.AvailableCategories = await _bookService.GetAllCategoriesAsync();
                        return View(viewModel);
                    }

                    TempData["SuccessMessage"] = $"Successfully added the book: {viewModel.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while adding the book: {ex.Message}";
                }
            }

            viewModel.AvailableAuthors = await _authorService.GetAllAuthorsAsync();
            viewModel.AvailablePublishers = await _bookService.GetAllPublishersAsync();
            viewModel.AvailableCategories = await _bookService.GetAllCategoriesAsync();
            TempData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Please fix the errors and try again.";
            return View(viewModel);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
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
                book.SelectedCategoryIds = book.AvailableCategories.Select(c => c.CategoryId).ToList();
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book for editing: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

                    var success = await _bookService.UpdateBookAsync(id, viewModel, coverImagePath);
                    if (!success)
                    {
                        TempData["ErrorMessage"] = $"No book found with ID {id}.";
                        return View("NotFound");
                    }

                    TempData["SuccessMessage"] = $"Successfully updated the book: {viewModel.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the book: {ex.Message}";
                }
            }

            viewModel.AvailableAuthors = await _authorService.GetAllAuthorsAsync();
            viewModel.AvailablePublishers = await _bookService.GetAllPublishersAsync();
            viewModel.AvailableCategories = await _bookService.GetAllCategoriesAsync();
            TempData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Please fix the errors and try again.";
            return View(viewModel);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for deletion.";
                return View("NotFound");
            }

            try
            {
                var book = await _bookService.GetBookDetailsAsync(id.Value);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book for deletion: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return View("NotFound");
                }

                var success = await _bookService.DeleteBookAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = $"Failed to delete book with ID {id}.";
                    return View("NotFound");
                }

                TempData["SuccessMessage"] = "Book deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the book: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Books/Loan/5
        public IActionResult Loan(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loan.";
                return View("NotFound");
            }

            return RedirectToAction("Create", "Loans", new { bookId = id });
        }
    }
}