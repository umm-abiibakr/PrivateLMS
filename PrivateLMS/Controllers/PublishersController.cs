using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using PrivateLMS.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    public class PublishersController : Controller
    {
        private readonly IPublisherService _publisherService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PublishersController(IPublisherService publisherService, IWebHostEnvironment webHostEnvironment)
        {
            _publisherService = publisherService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var publishers = await _publisherService.GetAllPublishersAsync();
                return View(publishers);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading publishers: {ex.Message}";
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Publisher ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var publisher = await _publisherService.GetPublisherDetailsAsync(id.Value);
                if (publisher == null)
                {
                    TempData["ErrorMessage"] = "Publisher not found.";
                    return View("NotFound");
                }
                return View(publisher);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading publisher details: {ex.Message}";
                return View("Error");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublisherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string? logoImagePath = null;
                if (model.LogoImage != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/publisher-logos");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoImage.CopyToAsync(stream);
                    }
                    logoImagePath = $"/images/publisher-logos/{fileName}";
                }

                var success = await _publisherService.CreatePublisherAsync(model, logoImagePath);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Failed to create publisher.";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Publisher added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the publisher: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Publisher ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var publisher = await _publisherService.GetPublisherDetailsAsync(id.Value);
                if (publisher == null)
                {
                    TempData["ErrorMessage"] = "Publisher not found.";
                    return View("NotFound");
                }
                return View(publisher);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading publisher for edit: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PublisherViewModel model)
        {
            if (id != model.PublisherId)
            {
                TempData["ErrorMessage"] = "Publisher ID mismatch.";
                return View("NotFound");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string? logoImagePath = null;
                if (model.LogoImage != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/publisher-logos");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoImage.CopyToAsync(stream);
                    }
                    logoImagePath = $"/images/publisher-logos/{fileName}";
                }

                var success = await _publisherService.UpdatePublisherAsync(id, model, logoImagePath);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Publisher not found.";
                    return View("NotFound");
                }

                TempData["SuccessMessage"] = "Publisher updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the publisher: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Publisher ID was not provided.";
                return View("NotFound");
            }

            try
            {
                var publisher = await _publisherService.GetPublisherDetailsAsync(id.Value);
                if (publisher == null)
                {
                    TempData["ErrorMessage"] = "Publisher not found.";
                    return View("NotFound");
                }
                return View(publisher);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading publisher for deletion: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _publisherService.DeletePublisherAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Publisher not found.";
                    return View("NotFound");
                }

                TempData["SuccessMessage"] = "Publisher deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the publisher: {ex.Message}";
                return View("Error");
            }
        }
    }
}