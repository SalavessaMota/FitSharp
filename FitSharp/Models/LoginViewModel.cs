using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [EmailAddress]
        public string Username { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}