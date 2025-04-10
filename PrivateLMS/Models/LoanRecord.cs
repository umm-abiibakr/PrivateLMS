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

        [Required]
        public int UserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ReturnDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fine amount cannot be negative.")]
        public decimal FineAmount { get; set; }

        public bool IsFinePaid { get; set; }

        public bool IsRenewed { get; set; }

        public Book Book { get; set; } = null!; 

        public ApplicationUser User { get; set; } = null!; // Non-nullable with null-forgiving operator
    }
}