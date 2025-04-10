using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class ReturnViewModel
    {
        public int LoanRecordId { get; set; }

        [Display(Name = "Book Title")]
        public string? BookTitle { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Loaner Name")]
        public string? LoanerName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Loan Date")]
        public DateTime? LoanDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
    }
}