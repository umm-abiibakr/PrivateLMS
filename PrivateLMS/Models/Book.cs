using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Book
    {
        [BindNever]
        public int BookId { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please select a Language")]
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Please select an Author")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "The ISBN field is required.")]
        [RegularExpression(@"^(97(8|9))?\d{9}(\d|X)$", ErrorMessage = "ISBN must be a valid 10 or 13 digit number.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "The Published Date field is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        public string? Description { get; set; } 

        [Range(0, int.MaxValue, ErrorMessage = "Available copies must be a non-negative number.")]
        public int AvailableCopies { get; set; }

        [BindNever]
        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        public string? CoverImagePath { get; set; } 

        // Navigation Properties
        [BindNever]
        public ICollection<LoanRecord>? LoanRecords { get; set; }

        public ICollection<BookCategory>? BookCategories { get; set; }

        public int? PublisherId { get; set; } 

        public Publisher? Publisher { get; set; }

        public Author Author { get; set; }
        public Language Language { get; set; }
    }
}