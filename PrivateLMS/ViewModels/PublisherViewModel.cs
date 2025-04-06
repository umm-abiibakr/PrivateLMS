using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class PublisherViewModel
    {
        public int PublisherId { get; set; }

        [Required(ErrorMessage = "Please enter Publisher Name")]
        [StringLength(100, ErrorMessage = "Publisher name cannot exceed 100 characters.")]
        public string PublisherName { get; set; }

        [Required(ErrorMessage = "Please enter Publisher Location")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; }

        public string? LogoImagePath { get; set; }
        [Display(Name = "Publisher Logo")]
        public IFormFile? LogoImage { get; set; } 
    }
}