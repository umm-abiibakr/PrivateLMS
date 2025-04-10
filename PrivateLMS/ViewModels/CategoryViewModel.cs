using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PrivateLMS.ViewModels 
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter the category name.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Number of Books")]
        public int BookCount { get; set; }

        public List<string> Books { get; set; } = new List<string>(); // For Details page
    }
}