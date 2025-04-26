using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var pagedAuthors = await _authorService.GetPagedAuthorsAsync(page, pageSize);
                var viewModels = pagedAuthors.Items.Select(a => new AuthorViewModel
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    Biography = a.Biography,
                    BirthDate = a.BirthDate,
                    DeathDate = a.DeathDate,
                    BookCount = a.Books.Count
                }).ToList();

                var pagedViewModel = new PagedResultViewModel<AuthorViewModel>
                {
                    Items = viewModels,
                    CurrentPage = pagedAuthors.CurrentPage,
                    PageSize = pagedAuthors.PageSize,
                    TotalItems = pagedAuthors.TotalItems,
                    TotalPages = pagedAuthors.TotalPages
                };

                return View(pagedViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading authors: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Author ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var author = await _authorService.GetAuthorByIdAsync(id.Value);
                if (author == null)
                {
                    TempData["ErrorMessage"] = $"No author found with ID {id}.";
                    return PartialView("_NotFound");
                }
                var viewModel = new AuthorViewModel
                {
                    AuthorId = author.AuthorId,
                    Name = author.Name,
                    Biography = author.Biography,
                    BirthDate = author.BirthDate,
                    DeathDate = author.DeathDate,
                    BookCount = author.Books.Count
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading author details: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new AuthorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AuthorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var author = new Author
                    {
                        Name = viewModel.Name,
                        Biography = viewModel.Biography,
                        BirthDate = viewModel.BirthDate,
                        DeathDate = viewModel.DeathDate
                    };
                    var success = await _authorService.CreateAuthorAsync(author);
                    if (!success)
                    {
                        TempData["ErrorMessage"] = "Failed to create the author.";
                        return View(viewModel);
                    }

                    TempData["SuccessMessage"] = $"Successfully added the author: {author.Name}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while adding the author: {ex.Message}";
                }
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Author ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var author = await _authorService.GetAuthorByIdAsync(id.Value);
                if (author == null)
                {
                    TempData["ErrorMessage"] = $"No author found with ID {id}.";
                    return PartialView("_NotFound");
                }
                var viewModel = new AuthorViewModel
                {
                    AuthorId = author.AuthorId,
                    Name = author.Name,
                    Biography = author.Biography,
                    BirthDate = author.BirthDate,
                    DeathDate = author.DeathDate,
                    BookCount = author.Books.Count
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the author for editing: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, AuthorViewModel viewModel)
        {
            if (id != viewModel.AuthorId)
            {
                TempData["ErrorMessage"] = "Author ID mismatch.";
                return PartialView("_NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var author = new Author
                    {
                        AuthorId = viewModel.AuthorId,
                        Name = viewModel.Name,
                        Biography = viewModel.Biography,
                        BirthDate = viewModel.BirthDate,
                        DeathDate = viewModel.DeathDate
                    };
                    var success = await _authorService.UpdateAuthorAsync(id, author);
                    if (!success)
                    {
                        TempData["ErrorMessage"] = $"No author found with ID {id}.";
                        return PartialView("_NotFound");
                    }

                    TempData["SuccessMessage"] = $"Successfully updated the author: {author.Name}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the author: {ex.Message}";
                }
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Author ID was not provided for deletion.";
                return PartialView("_NotFound");
            }

            try
            {
                var author = await _authorService.GetAuthorByIdAsync(id.Value);
                if (author == null)
                {
                    TempData["ErrorMessage"] = $"No author found with ID {id} for deletion.";
                    return PartialView("_NotFound");
                }
                var viewModel = new AuthorViewModel
                {
                    AuthorId = author.AuthorId,
                    Name = author.Name,
                    Biography = author.Biography,
                    BirthDate = author.BirthDate,
                    DeathDate = author.DeathDate,
                    BookCount = author.Books.Count
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading the author for deletion: {ex.Message}";
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
                var success = await _authorService.DeleteAuthorAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = $"Failed to delete author with ID {id}. It may have associated books.";
                    return PartialView("_NotFound");
                }

                TempData["SuccessMessage"] = "Author deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the author: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}