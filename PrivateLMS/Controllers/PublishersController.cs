using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace PrivateLMS.Controllers
{
    public class PublishersController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PublishersController(LibraryDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var publishers = await _context.Publishers
                .Select(p => new PublisherViewModel
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Location = p.Location,
                    LogoImagePath = p.LogoImagePath
                })
                .ToListAsync();
            return View(publishers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Publisher ID was not provided.";
                return View("NotFound");
            }

            var publisher = await _context.Publishers
                .Select(p => new PublisherViewModel
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Location = p.Location,
                    LogoImagePath = p.LogoImagePath
                })
                .FirstOrDefaultAsync(p => p.PublisherId == id);

            if (publisher == null)
            {
                TempData["ErrorMessage"] = "Publisher not found.";
                return View("NotFound");
            }

            return View(publisher);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublisherViewModel model)
        {
            if (ModelState.IsValid)
            {
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

                    var publisher = new Publisher
                    {
                        PublisherName = model.PublisherName,
                        Location = model.Location,
                        LogoImagePath = logoImagePath
                    };
                    _context.Publishers.Add(publisher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Publisher added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while creating the publisher: {ex.Message}";
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Publisher ID was not provided.";
                return View("NotFound");
            }

            var publisher = await _context.Publishers
                .Select(p => new PublisherViewModel
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Location = p.Location,
                    LogoImagePath = p.LogoImagePath
                })
                .FirstOrDefaultAsync(p => p.PublisherId == id);

            if (publisher == null)
            {
                TempData["ErrorMessage"] = "Publisher not found.";
                return View("NotFound");
            }

            return View(publisher);
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

            if (ModelState.IsValid)
            {
                try
                {
                    var publisher = await _context.Publishers.FindAsync(id);
                    if (publisher == null)
                    {
                        TempData["ErrorMessage"] = "Publisher not found.";
                        return View("NotFound");
                    }

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
                        if (!string.IsNullOrEmpty(publisher.LogoImagePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, publisher.LogoImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        publisher.LogoImagePath = $"/images/publisher-logos/{fileName}";
                    }

                    publisher.PublisherName = model.PublisherName;
                    publisher.Location = model.Location;
                    _context.Update(publisher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Publisher updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred while updating the publisher: {ex.Message}";
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Publisher ID was not provided.";
                return View("NotFound");
            }

            var publisher = await _context.Publishers
                .Select(p => new PublisherViewModel
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Location = p.Location,
                    LogoImagePath = p.LogoImagePath
                })
                .FirstOrDefaultAsync(p => p.PublisherId == id);

            if (publisher == null)
            {
                TempData["ErrorMessage"] = "Publisher not found.";
                return View("NotFound");
            }

            return View(publisher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                TempData["ErrorMessage"] = "Publisher not found.";
                return View("NotFound");
            }

            if (!string.IsNullOrEmpty(publisher.LogoImagePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, publisher.LogoImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Publisher deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}