using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Fine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fine amount cannot be negative.")]
        public decimal Amount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime IssuedDate { get; set; }

        public bool IsPaid { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public LoanRecord LoanRecord { get; set; } = null!;
    }
}