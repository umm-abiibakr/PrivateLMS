namespace PrivateLMS.ViewModels
{
    public class UserDashboardViewModel
    {
        public List<LoanViewModel> ActiveLoans { get; set; } = new List<LoanViewModel>();
        public List<FineViewModel> Fines { get; set; } = new List<FineViewModel>();
        public List<BookReviewViewModel> RecentRatings { get; set; } = new List<BookReviewViewModel>();
    }
}