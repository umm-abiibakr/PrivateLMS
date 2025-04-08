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
    //[Authorize(Roles = "Admin")]
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            try
            {
                var authors = await _authorService.GetAllAuthorsAsync();
                var viewModels = authors.Select(a => new AuthorViewModel
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    Biography = a.Biography,
                    BirthDate = a.BirthDate,
                    DeathDate = a.DeathDate,
                    BookCount = a.Books.Count
                }).ToList();
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading authors: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Author ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var author = await _authorService.GetAuthorByIdAsync(id.Value);
                if (author == null)
                {
                    TempData["ErrorMessage"] = $"No author found with ID {id}.";
                    return View("NotFound");
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

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View(new AuthorViewModel());
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Author ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var author = await _authorService.GetAuthorByIdAsync(id.Value);
                if (author == null)
                {
                    TempData["ErrorMessage"] = $"No author found with ID {id}.";
                    return View("NotFound");
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

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuthorViewModel viewModel)
        {
            if (id != viewModel.AuthorId)
            {
                TempData["ErrorMessage"] = "Author ID mismatch.";
                return View("NotFound");
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
                        return View("NotFound");
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

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Author ID was not provided for deletion.";
                return View("NotFound");
            }

            try
            {
                var author = await _authorService.GetAuthorByIdAsync(id.Value);
                if (author == null)
                {
                    TempData["ErrorMessage"] = $"No author found with ID {id} for deletion.";
                    return View("NotFound");
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

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _authorService.DeleteAuthorAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = $"Failed to delete author with ID {id}. It may have associated books.";
                    return View("NotFound");
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