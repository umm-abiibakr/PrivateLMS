using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please enter the book title.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter the author's name.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Please enter the ISBN.")]
        [RegularExpression(@"^\d{3}-\d{10}$", ErrorMessage = "ISBN must be in the format XXX-XXXXXXXXXX.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        [StringLength(50, ErrorMessage = "Language cannot exceed 50 characters.")]
        public string Language { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        public List<Category> AvailableCategories { get; set; } = new List<Category>();

        // Adding LoanRecords to the ViewModel
        public List<LoanRecord> LoanRecords { get; set; } = new List<LoanRecord>();

        // A property to directly display category names
        public List<string> CategoryNames { get; set; } = new List<string>();

        // A formatted property to display categories as a single string
        public string DisplayCategories => string.Join(", ", CategoryNames);
    }
}
