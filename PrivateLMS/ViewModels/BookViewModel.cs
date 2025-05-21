using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using PrivateLMS.Models;

namespace PrivateLMS.ViewModels
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Language")]
        [Display(Name = "Language")]
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Please select an Author")]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "The ISBN field is required.")]
        [RegularExpression(@"^(97(8|9))?\d{9}(\d|X)$", ErrorMessage = "ISBN must be a valid 10 or 13 digit number.")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Published Date field is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Available copies must be a non-negative number.")]
        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        public string? CoverImagePath { get; set; }

        [Display(Name = "Book Cover")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "Publisher")]
        public int? PublisherId { get; set; }

        [Required(ErrorMessage = "Please select at least one category")]
        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public List<Language> AvailableLanguages { get; set; } = new List<Language>();
        public List<Author> AvailableAuthors { get; set; } = new List<Author>();
        public List<Publisher> AvailablePublishers { get; set; } = new List<Publisher>();
        public List<Category> AvailableCategories { get; set; } = new List<Category>();
        public List<LoanRecord> LoanRecords { get; set; } = new List<LoanRecord>();
        public float AverageRating { get; set; } // Average rating (1-5)
        public int RatingCount { get; set; } // Number of ratings
        public float UserRating { get; set; } // User's rating (0 if not rated)
        public string UserReview { get; set; } = string.Empty;
        public List<BookReviewViewModel> Reviews { get; set; } = new();
        public int TotalLoans { get; set; }
    }
}