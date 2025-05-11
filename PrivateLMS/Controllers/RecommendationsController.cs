using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using PrivateLMS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Controllers
{
    [Authorize]
    public class RecommendationsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecommendationService _recommendationService;

        public RecommendationsController(
            LibraryDbContext context,
            UserManager<ApplicationUser> userManager,
            RecommendationService recommendationService)
        {
            _context = context;
            _userManager = userManager;
            _recommendationService = recommendationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var preferencesExist = await _context.UserPreferences
                .AnyAsync(p => p.UserId == userId && p.PreferencesSetup);

            var recommendations = await GetUserRecommendations(userId);

            if (!preferencesExist || !recommendations.Any())
            {
                ViewBag.ShowPreferencesModal = true;
            }

            await LoadPreferencesIntoViewBag(userId);

            return View(recommendations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePreferences(int[] selectedCategories, int[] selectedAuthors, int[] selectedLanguages)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var hasSelectedAny = selectedCategories.Any() || selectedAuthors.Any() || selectedLanguages.Any();
            if (!hasSelectedAny)
            {
                TempData["PreferenceError"] = "Please select at least one preference category.";
                ViewBag.ShowPreferencesModal = true;
                await LoadPreferencesIntoViewBag(userId);
                return RedirectToAction(nameof(Index));
            }

            var userPrefs = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userPrefs == null)
            {
                userPrefs = new UserPreference { UserId = userId };
                _context.UserPreferences.Add(userPrefs);
            }

            userPrefs.PreferencesSetup = true;

            _context.CategoryPreferences.RemoveRange(
                _context.CategoryPreferences.Where(p => p.UserId == userId));
            _context.AuthorPreferences.RemoveRange(
                _context.AuthorPreferences.Where(p => p.UserId == userId));
            _context.LanguagePreferences.RemoveRange(
                _context.LanguagePreferences.Where(p => p.UserId == userId));

            _context.CategoryPreferences.AddRange(selectedCategories.Select(catId =>
                new CategoryPreference { UserId = userId, CategoryId = catId }));

            _context.AuthorPreferences.AddRange(selectedAuthors.Select(authorId =>
                new AuthorPreference { UserId = userId, AuthorId = authorId }));

            _context.LanguagePreferences.AddRange(selectedLanguages.Select(langId =>
                new LanguagePreference { UserId = userId, LanguageId = langId }));

            await _context.SaveChangesAsync();
            TempData["PreferenceSaved"] = true;

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPreferencesIntoViewBag(int userId)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Authors = await _context.Authors.ToListAsync();
            ViewBag.Languages = await _context.Languages.ToListAsync();

            ViewBag.SelectedCategories = await _context.CategoryPreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.CategoryId)
                .ToListAsync();

            ViewBag.SelectedAuthors = await _context.AuthorPreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.AuthorId)
                .ToListAsync();

            ViewBag.SelectedLanguages = await _context.LanguagePreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.LanguageId)
                .ToListAsync();
        }

        private async Task<List<BookRecommendationViewModel>> GetUserRecommendations(int userId)
        {
            var books = await _context.Books
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.Author)
                .Include(b => b.Language)
                .ToListAsync();

            var userCategoryPrefs = await _context.CategoryPreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.CategoryId)
                .ToListAsync();

            var userAuthorPrefs = await _context.AuthorPreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.AuthorId)
                .ToListAsync();

            var userLanguagePrefs = await _context.LanguagePreferences
                .Where(p => p.UserId == userId)
                .Select(p => p.LanguageId)
                .ToListAsync();

            var recommendations = new List<BookRecommendationViewModel>();

            foreach (var book in books)
            {
                float categoryMatch = book.BookCategories != null &&
                    book.BookCategories.Any(bc => userCategoryPrefs.Contains(bc.CategoryId)) ? 1.0f : 0.0f;

                float authorMatch = userAuthorPrefs.Contains(book.AuthorId) ? 1.0f : 0.0f;
                float languageMatch = userLanguagePrefs.Contains(book.LanguageId) ? 1.0f : 0.0f;

                float score = await _recommendationService.GetRecommendationScoreAsync(
                    categoryMatch, authorMatch, languageMatch);

                if (score >= 0.3f) // Use 0.3 to show more books with friendlier labels
                {
                    recommendations.Add(new BookRecommendationViewModel
                    {
                        Book = book,
                        RecommendationScore = score
                    });
                }
            }

            return recommendations
                .OrderByDescending(r => r.RecommendationScore)
                .ToList();
        }
    }
}
