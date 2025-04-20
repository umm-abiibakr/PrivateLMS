using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class Step4ViewModel
    {
        [Required(ErrorMessage = "You must accept the terms to register.")]
        [Display(Name = "I understand and accept the terms and conditions")]
        public bool TermsAccepted { get; set; }
    }
}