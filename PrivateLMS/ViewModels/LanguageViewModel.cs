using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PrivateLMS.ViewModels
{
    public class LanguageViewModel
    {
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Please enter the language.")]
        [StringLength(100, ErrorMessage = "Language cannot exceed 100 characters.")]
        [Display(Name = "Language Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Number of Books")]
        public int BookCount { get; set; }
        public List<string> Books { get; set; } = new List<string>(); // For Details page
    }
}
