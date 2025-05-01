using PrivateLMS.Models;

namespace PrivateLMS.ViewModels
{
    public class UserDashboardViewModel
    {
        public PagedResultViewModel<LoanViewModel> ActiveLoans { get; set; } = new PagedResultViewModel<LoanViewModel> { Items = new List<LoanViewModel>() };
        public PagedResultViewModel<FineViewModel> Fines { get; set; } = new PagedResultViewModel<FineViewModel> { Items = new List<FineViewModel>() };
        public PagedResultViewModel<BookReviewViewModel> RecentRatings { get; set; } = new PagedResultViewModel<BookReviewViewModel> { Items = new List<BookReviewViewModel>() };
    }
}