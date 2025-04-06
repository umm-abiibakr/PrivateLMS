using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.Models;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading categories: {ex.Message}";
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Category ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id.Value);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return View("NotFound");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category details: {ex.Message}";
                return View("Error");
            }
        }

        public IActionResult Create()
        {
            return View();
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
            if (id == null)
            {
                TempData["ErrorMessage"] = "Category ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id.Value);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return View("NotFound");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category for edit: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.CategoryId)
            {
                TempData["ErrorMessage"] = "Category ID mismatch.";
                return View("NotFound");
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
                    return View("NotFound");
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
            if (id == null)
            {
                TempData["ErrorMessage"] = "Category ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id.Value);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return View("NotFound");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading category for deletion: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Category not found or cannot be deleted due to associated books.";
                    return View("NotFound");
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
                return View("Error");
            }
        }
    }
}