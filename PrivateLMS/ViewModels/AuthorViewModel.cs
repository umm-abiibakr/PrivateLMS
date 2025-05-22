using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class AuthorViewModel
    {
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "The Author Name is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Biography cannot exceed 1000 characters.")]
        public string? Biography { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DeathDate { get; set; }

        [Display(Name = "Number of Books")]
        public int BookCount { get; set; }

        public List<string> Books { get; set; } = new List<string>();
    }
}