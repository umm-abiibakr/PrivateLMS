using PrivateLMS.ViewModels;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface IBookRatingService
    {
        Task<bool> RateBookAsync(BookRatingViewModel model, int userId);
        Task<float> GetAverageRatingAsync(int bookId);
        Task<int> GetRatingCountAsync(int bookId);
        Task<float> GetUserRatingAsync(int bookId, int userId);
    }
}