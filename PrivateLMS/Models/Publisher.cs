using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Publisher
    {
        [Key]
        public int PublisherId { get; set; }

        [Required(ErrorMessage = "Please enter Publisher Name")]
        public string PublisherName { get; set; }

        [Required(ErrorMessage = "Please enter Publisher Location")]
        public string Location { get; set; }

        public string? LogoImagePath { get; set; } 
    }
}