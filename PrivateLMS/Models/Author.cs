using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "The Author Name is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Name { get; set; }
        public string? Biography { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? DeathDate { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}