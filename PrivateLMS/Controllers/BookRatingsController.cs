using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize]
    public class BookRatingsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IBookRatingService _bookRatingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookRatingsController(LibraryDbContext context, IBookRatingService bookRatingService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _bookRatingService = bookRatingService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(BookRatingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please provide a rating between 1 and 5.";
                    return RedirectToAction("Details", "Books", new { id = model.BookId });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to rate a book.";
                    return RedirectToAction("Index", "Login");
                }

                var bookExists = await _context.Books.AnyAsync(b => b.BookId == model.BookId);
                if (!bookExists)
                {
                    TempData["ErrorMessage"] = "The book you're trying to rate does not exist.";
                    return RedirectToAction("Details", "Books", new { id = model.BookId });
                }

                var success = await _bookRatingService.RateBookAsync(model, user.Id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "We couldn't save your rating. Please try again.";
                    return RedirectToAction("Details", "Books", new { id = model.BookId });
                }

                TempData["SuccessMessage"] = "Thank you for your rating!";
                return RedirectToAction("Details", "Books", new { id = model.BookId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong while saving your rating. Please try again.";
                return RedirectToAction("Details", "Books", new { id = model.BookId });
            }
        }


    }
}