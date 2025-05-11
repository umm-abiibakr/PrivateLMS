using PrivateLMS.Models;

namespace PrivateLMS.ViewModels
{
    public class BookRecommendationViewModel
    {
        public Book Book { get; set; } = null!;
        public float RecommendationScore { get; set; }
    }
}
