using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;

namespace PrivateLMS.Controllers
{
    public class PublishersController : Controller
    {
        private readonly LibraryDbContext _context;

        public PublishersController(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var publishers = await _context.Publishers
                .Select(p => new PublisherViewModel
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Location = p.Location
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
                    Location = p.Location
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
                var publisher = new Publisher
                {
                    PublisherName = model.PublisherName,
                    Location = model.Location
                };
                _context.Publishers.Add(publisher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Publisher added successfully.";
                return RedirectToAction(nameof(Index));
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
                    Location = p.Location
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
                var publisher = await _context.Publishers.FindAsync(id);
                if (publisher == null)
                {
                    TempData["ErrorMessage"] = "Publisher not found.";
                    return View("NotFound");
                }

                publisher.PublisherName = model.PublisherName;
                publisher.Location = model.Location;
                _context.Update(publisher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Publisher updated successfully.";
                return RedirectToAction(nameof(Index));
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
                    Location = p.Location
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
        public async Task<IActionResult> Delete(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                TempData["ErrorMessage"] = "Publisher not found.";
                return View("NotFound");
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Publisher deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}