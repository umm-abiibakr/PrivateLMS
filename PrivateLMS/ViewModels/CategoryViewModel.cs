using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter the category name.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string CategoryName { get; set; }

        public int BookCount { get; set; }

        public List<string> Books { get; set; } = new List<string>(); // Added for Details page
    }
}