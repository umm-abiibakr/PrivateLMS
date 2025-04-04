﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        [Required(ErrorMessage = "The Language field is required.")]
        [StringLength(100, ErrorMessage = "Language name cannot exceed 100 characters.")]
        public string Language { get; set; }

        [Required(ErrorMessage = "The Author field is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "The ISBN field is required.")]
        [RegularExpression(@"^\d{3}-\d{10}$", ErrorMessage = "ISBN must be in the format XXX-XXXXXXXXXX.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "The Published Date field is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        [BindNever]
        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true; 

        // Navigation Property
        [BindNever]
        public ICollection<LoanRecord>? LoanRecords { get; set; }
        public ICollection<Publisher>? Publishers { get; set; }
        public ICollection<Category>? Categories { get; set; }

    }
}
