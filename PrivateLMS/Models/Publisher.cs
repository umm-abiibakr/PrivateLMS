using Microsoft.AspNetCore.Mvc.ModelBinding;
using PrivateLMS.Models;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Publisher
    {
        [Key]
        public int PublisherId { get; set; } //PK

        [Required]
        public int BookId { get; set; } //FK

        [Required(ErrorMessage = "Please enter Publisher Name")]
        public string PublisherName { get; set; }

        [Required(ErrorMessage = "Please enter Publisher Location")]
        public string Location { get; set; }

        // Navigation Properties
        [BindNever]
        public Book Book { get; set; }
    }
}