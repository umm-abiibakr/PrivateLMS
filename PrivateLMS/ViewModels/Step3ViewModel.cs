using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PrivateLMS.ViewModels
{
    public class Step3ViewModel
    {
        [Required(ErrorMessage = "The Phone Number field is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Address field is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "The City field is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "The State field is required.")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Postal Code field is required.")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Country field is required.")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        // Lists for dropdowns
        public List<string> Countries { get; set; } = new List<string>();
        public List<string> States { get; set; } = new List<string>();
    }
}