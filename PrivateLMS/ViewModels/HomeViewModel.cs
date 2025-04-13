namespace PrivateLMS.ViewModels
{
    public class HomeViewModel
    {
        public List<BookViewModel> NewBooks { get; set; } = new List<BookViewModel>();
        public List<BookViewModel> PopularBooks { get; set; } = new List<BookViewModel>();
        public List<BookReviewViewModel> RecentReviews { get; set; } = new List<BookReviewViewModel>();
        public List<BookViewModel> Recommendations { get; set; } = new List<BookViewModel>();
        public List<LoanViewModel> OverdueLoans { get; set; } = new List<LoanViewModel>();
    }
}