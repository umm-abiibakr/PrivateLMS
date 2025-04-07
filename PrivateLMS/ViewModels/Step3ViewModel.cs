using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class Step3ViewModel
    {
        [Phone]
        [Required(ErrorMessage = "The PhoneNumber field is required.")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "The Email field is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Address field is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "The City field is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "The State field is required.")]
        public string State { get; set; }

        [Required(ErrorMessage = "The PostalCode field is required.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "The Country field is required.")]
        public string Country { get; set; }
    }
}