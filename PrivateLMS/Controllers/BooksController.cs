using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(
            IBookService bookService,
            IAuthorService authorService,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _bookService = bookService;
            _authorService = authorService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchTerm = "")
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

        public async Task<IActionResult> Recommendations()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to view recommendations.";
                    return RedirectToAction("Index", "Login");
                }

                var recommendations = await _bookService.GetRecommendedBooksAsync(user.Id);
                return View(recommendations);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading recommendations: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var book = await _bookService.GetBookDetailsAsync(id.Value);
                if (book is null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return PartialView("_NotFound");
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book details: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new BookViewModel
                {
                    AvailableAuthors = await _authorService.GetAllAuthorsAsync() ?? new List<Author>(),
                    AvailablePublishers = await _bookService.GetAllPublishersAsync() ?? new List<Publisher>(),
                    AvailableCategories = await _bookService.GetAllCategoriesAsync() ?? new List<Category>()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the create form: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

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
                    if (viewModel.CoverImage is not null)
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
                        await PopulateBookViewModelLists(viewModel);
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

            await PopulateBookViewModelLists(viewModel);
            TempData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Please fix the errors and try again.";
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var book = await _bookService.GetBookDetailsAsync(id.Value);
                if (book is null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return PartialView("_NotFound");
                }
                book.SelectedCategoryIds = book.AvailableCategories?.Select(c => c.CategoryId).ToList() ?? new List<int>();
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book for editing: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.BookId)
            {
                TempData["ErrorMessage"] = "Book ID mismatch.";
                return PartialView("_NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string? coverImagePath = null;
                    if (viewModel.CoverImage is not null)
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
                        return PartialView("_NotFound");
                    }

                    TempData["SuccessMessage"] = $"Successfully updated the book: {viewModel.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the book: {ex.Message}";
                }
            }

            await PopulateBookViewModelLists(viewModel);
            TempData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Please fix the errors and try again.";
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for deletion.";
                return PartialView("_NotFound");
            }

            try
            {
                var book = await _bookService.GetBookDetailsAsync(id.Value);
                if (book is null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return PartialView("_NotFound");
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the book for deletion: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book is null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return PartialView("_NotFound");
                }

                var success = await _bookService.DeleteBookAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = $"Failed to delete book with ID {id}.";
                    return PartialView("_NotFound");
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

        public async Task<IActionResult> Loan(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for loan.";
                return PartialView("_NotFound");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                TempData["ErrorMessage"] = "You must be logged in to loan a book.";
                return RedirectToAction("Index", "Login");
            }

            return RedirectToAction("Create", "Loans", new { bookId = id });
        }

        private async Task PopulateBookViewModelLists(BookViewModel viewModel)
        {
            viewModel.AvailableAuthors = await _authorService.GetAllAuthorsAsync() ?? new List<Author>();
            viewModel.AvailablePublishers = await _bookService.GetAllPublishersAsync() ?? new List<Publisher>();
            viewModel.AvailableCategories = await _bookService.GetAllCategoriesAsync() ?? new List<Category>();
        }
    }
}