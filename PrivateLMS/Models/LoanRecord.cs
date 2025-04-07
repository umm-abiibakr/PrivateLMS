using System;
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

        [DataType(DataType.DateTime)]
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; } 

        [DataType(DataType.DateTime)]
        public DateTime? ReturnDate { get; set; }

        public Book Book { get; set; }
    }
}