using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The Language field is required.")]
        [StringLength(100, ErrorMessage = "Language name cannot exceed 100 characters.")]
        public string Language { get; set; }

        [Required(ErrorMessage = "The Author field is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "The ISBN field is required.")]
        [RegularExpression(@"^(97(8|9))?\d{9}(\d|X)$", ErrorMessage = "ISBN must be a valid 10 or 13 digit number.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "The Published Date field is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Available copies must be a non-negative number.")]
        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        public string? CoverImagePath { get; set; }
        [Display(Name = "Book Cover")]
        public IFormFile? CoverImage { get; set; }

        [Required(ErrorMessage = "Please select a Publisher")]
        [Display(Name = "Publisher")]
        public int? PublisherId { get; set; }

        [Required(ErrorMessage = "Please select at least one category")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        public List<Publisher> AvailablePublishers { get; set; } = new List<Publisher>();
        public List<Category> AvailableCategories { get; set; } = new List<Category>();
        public List<LoanRecord> LoanRecords { get; set; } = new List<LoanRecord>();
    }
}