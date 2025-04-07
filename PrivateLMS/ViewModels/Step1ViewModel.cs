using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class Step1ViewModel
    {
        [Required(ErrorMessage = "Please enter your first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please select your gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please enter your date of birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }
}