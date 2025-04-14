using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class BookRatingViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public float Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Review cannot exceed 1000 characters.")]
        public string? Review { get; set; } = string.Empty;
    }
}