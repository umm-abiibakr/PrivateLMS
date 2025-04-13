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

        public bool IsRenewed { get; set; }

        public Book Book { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        public ICollection<Fine> Fines { get; set; } = new List<Fine>(); 
    }
}