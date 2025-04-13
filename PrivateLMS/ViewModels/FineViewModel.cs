using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class FineViewModel
    {
        public int Id { get; set; }

        public int LoanRecordId { get; set; }

        [Display(Name = "Book Title")]
        public string BookTitle { get; set; } = string.Empty;

        [Display(Name = "Loaner Name")]
        public string LoanerName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Loan Date")]
        public DateTime LoanDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Issued Date")]
        public DateTime IssuedDate { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Fine Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Fine Paid")]
        public bool IsPaid { get; set; }
    }
}