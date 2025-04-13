using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class LoanViewModel
    {
        public int LoanRecordId { get; set; }

        public int BookId { get; set; }

        [Display(Name = "Book Title")]
        public string? BookTitle { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Loaner Name")]
        public string? LoanerName { get; set; }

        [Display(Name = "Loan Date")]
        public DateTime LoanDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Renewed")]
        public bool IsRenewed { get; set; }

        [Display(Name = "Days Overdue")]
        public int DaysOverdue { get; set; }
    }
}