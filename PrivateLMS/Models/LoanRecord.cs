using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class LoanRecord
    {
        [Key]
        public int LoanRecordId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please enter Loaner Name")]
        public string LoanerName { get; set; }

        [Required(ErrorMessage = "Please enter Loaner Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Address")]
        public string LoanerEmail { get; set; }

        [Required(ErrorMessage = "Please enter Loaner Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; }

        [BindNever]
        [DataType(DataType.DateTime)]
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? ReturnDate { get; set; }

        [BindNever]
        public Book Book { get; set; }
    }
}