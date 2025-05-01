namespace PrivateLMS.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int BooksLoanedOut { get; set; }
        public int PendingReturns { get; set; }
        public int OverdueLoans { get; set; }
        public int TotalUsers { get; set; }
        public int UnapprovedUsers { get; set; }
        public int BannedUsers { get; set; }
        public List<LoanViewModel> RecentLoans { get; set; }
        public List<BookViewModel> TopBorrowedBooks { get; set; }
        public LoanStatsViewModel LoanStats { get; set; }
        public UserStatsViewModel UserStats { get; set; }
    }
}