using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public class BookRatingService : IBookRatingService
    {
        private readonly LibraryDbContext _context;

        public BookRatingService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RateBookAsync(BookRatingViewModel model, int userId)
        {
            var existingRating = await _context.BookRatings
                .FirstOrDefaultAsync(br => br.BookId == model.BookId && br.UserId == userId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Rating = model.Rating;
                existingRating.Review = model.Review;
                existingRating.RatedOn = DateTime.UtcNow;
                _context.Update(existingRating);
            }
            else
            {
                // Create new rating
                var rating = new BookRating
                {
                    UserId = userId,
                    BookId = model.BookId,
                    Rating = model.Rating,
                    Review = model.Review,
                    RatedOn = DateTime.UtcNow
                };
                _context.BookRatings.Add(rating);
            }

            await _context.SaveChangesAsync();

            // Log the rating action
            var book = await _context.Books.FindAsync(model.BookId);
            _context.UserActivities.Add(new UserActivity
            {
                UserId = userId,
                Action = "RateBook",
                Timestamp = DateTime.UtcNow,
                Details = $"User rated book ID {model.BookId} (Title: {book?.Title ?? "Unknown"}) with {model.Rating} stars"
            });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<float> GetAverageRatingAsync(int bookId)
        {
            var ratings = await _context.BookRatings
                .Where(br => br.BookId == bookId)
                .ToListAsync();

            return ratings.Any() ? (float)ratings.Average(br => br.Rating) : 0f;
        }

        public async Task<int> GetRatingCountAsync(int bookId)
        {
            return await _context.BookRatings
                .CountAsync(br => br.BookId == bookId);
        }

        public async Task<float> GetUserRatingAsync(int bookId, int userId)
        {
            var rating = await _context.BookRatings
                .FirstOrDefaultAsync(br => br.BookId == bookId && br.UserId == userId);

            return rating?.Rating ?? 0f;
        }
    }
}