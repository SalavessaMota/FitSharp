using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class RegisterNewUserViewModel : AdminRegisterNewUserViewModel
    {
        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string Confirm { get; set; }
    }
}