using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class Language
    {
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(100, ErrorMessage = "Language cannot exceed 100 characters.")]
        public string Name { get; set; }
    }
}