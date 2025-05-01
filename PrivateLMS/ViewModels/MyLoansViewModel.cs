using PrivateLMS.Models;

namespace PrivateLMS.ViewModels
{
    public class MyLoansViewModel
    {
        public PagedResultViewModel<LoanViewModel> ActiveLoans { get; set; } = new PagedResultViewModel<LoanViewModel> { Items = new List<LoanViewModel>() };
        public PagedResultViewModel<LoanViewModel> PastLoans { get; set; } = new PagedResultViewModel<LoanViewModel> { Items = new List<LoanViewModel>() };
    }
}