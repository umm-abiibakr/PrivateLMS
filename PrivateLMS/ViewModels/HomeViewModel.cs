namespace PrivateLMS.ViewModels
{
    public class HomeViewModel
    {
        public List<BookViewModel> NewBooks { get; set; } = new List<BookViewModel>();
        public List<BookViewModel> PopularBooks { get; set; } = new List<BookViewModel>();
        public List<BookReviewViewModel> RecentReviews { get; set; } = new List<BookReviewViewModel>();
        public List<BookRecommendationViewModel> Recommendations { get; set; } = new List<BookRecommendationViewModel>();
        public List<LoanViewModel> OverdueLoans { get; set; } = new List<LoanViewModel>();
    }
}