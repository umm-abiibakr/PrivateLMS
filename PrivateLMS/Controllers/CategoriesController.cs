using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var pagedCategories = await _categoryService.GetPagedCategoriesAsync(page, pageSize);
                return View(pagedCategories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading categories: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Category ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id.Value);
                if (category is null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return PartialView("_NotFound");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category details: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _categoryService.CreateCategoryAsync(model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Failed to create category.";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Category added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the category: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Category ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id.Value);
                if (category is null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return PartialView("_NotFound");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category for edit: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.CategoryId)
            {
                TempData["ErrorMessage"] = "Category ID mismatch.";
                return PartialView("_NotFound");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _categoryService.UpdateCategoryAsync(id, model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return PartialView("_NotFound");
                }

                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the category: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Category ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id.Value);
                if (category is null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return PartialView("_NotFound");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category for deletion: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Category not found or cannot be deleted due to associated books.";
                    return PartialView("_NotFound");
                } 

                TempData["SuccessMessage"] = "Category deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the category: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }
    } 
}