using Microsoft.AspNetCore.Mvc.ModelBinding;
using PrivateLMS.Models;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; } //PK

        [Required]
        public int BookId { get; set; } //FK

        [Required(ErrorMessage = "Please enter Category Name")]
        public string CategoryName { get; set; }

        // Navigation Properties
        [BindNever]
        public Book Book { get; set; }
    }
}