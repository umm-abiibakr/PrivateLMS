using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class PayFineViewModel
    {
        public int FineId { get; set; }

        [Display(Name = "Book Title")]
        public string BookTitle { get; set; } = string.Empty;

        [Display(Name = "Loaner Name")]
        public string LoanerName { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        [Display(Name = "Fine Amount")]
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Issued Date")]
        public DateTime IssuedDate { get; set; }
    }
}