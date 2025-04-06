using Microsoft.AspNetCore.Mvc;
using PrivateLMS.Services;
using System.Threading.Tasks;

namespace PrivateLMS.ViewComponents
{
    public class LoanCartViewComponent : ViewComponent
    {
        private readonly ILoanService _loanService;

        public LoanCartViewComponent(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loans = await _loanService.GetAllLoansAsync();
            var activeLoanCount = loans.Count(l => l.ReturnDate == null); // Assuming ReturnDate added to LoanViewModel
            return View(activeLoanCount);
        }
    }
}