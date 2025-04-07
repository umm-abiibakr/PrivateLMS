using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class Step2ViewModel
    {
        [Required(ErrorMessage = "Please enter your username")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(50, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}