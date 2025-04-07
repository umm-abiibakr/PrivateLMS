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

        [Required(ErrorMessage = "Please enter Loaner Name")]
        public string? LoanerName { get; set; }

        [Required(ErrorMessage = "Please enter Loaner Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Address")]
        public string? LoanerEmail { get; set; }

        [Required(ErrorMessage = "Please enter Loaner Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string? Phone { get; set; }

        [Display(Name = "Loan Date")]
        public DateTime LoanDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; } 

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }
    }
}