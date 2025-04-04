using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class ReturnViewModel
    {
        [Required]
        public int LoanRecordId { get; set; }

        [BindNever]
        public string? BookTitle { get; set; }
        [BindNever]
        public string? LoanerName { get; set; }
        [BindNever]
        public DateTime? LoanDate { get; set; }
    }
}
