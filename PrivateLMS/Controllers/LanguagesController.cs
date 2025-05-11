using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LanguagesController : Controller
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var pagedLanguages = await _languageService.GetPagedLanguagesAsync(page, pageSize);
                return View(pagedLanguages);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading languages: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Language ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var language = await _languageService.GetLanguageDetailsAsync(id.Value);
                if (language is null)
                {
                    TempData["ErrorMessage"] = "Language not found.";
                    return PartialView("_NotFound");
                }
                return View(language);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading language details: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        public IActionResult Create()
        {
            return View(new LanguageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LanguageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _languageService.CreateLanguageAsync(model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Failed to create language.";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Language added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the language: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Language ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var language = await _languageService.GetLanguageDetailsAsync(id.Value);
                if (language is null)
                {
                    TempData["ErrorMessage"] = "Language not found.";
                    return PartialView("_NotFound");
                }
                return View(language);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading language for edit: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LanguageViewModel model)
        {
            if (id != model.LanguageId)
            {
                TempData["ErrorMessage"] = "Language ID mismatch.";
                return PartialView("_NotFound");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _languageService.UpdateLanguageAsync(id, model);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Language not found.";
                    return PartialView("_NotFound");
                }

                TempData["SuccessMessage"] = "Language updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the language: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Language ID was not provided.";
                return PartialView("_NotFound");
            }

            try
            {
                var language = await _languageService.GetLanguageDetailsAsync(id.Value);
                if (language is null)
                {
                    TempData["ErrorMessage"] = "Language not found.";
                    return PartialView("_NotFound");
                }
                return View(language);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading language for deletion: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _languageService.DeleteLanguageAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Language not found or cannot be deleted due to associated books.";
                    return PartialView("_NotFound");
                }

                TempData["SuccessMessage"] = "Language deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the language: {ex.Message}";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
