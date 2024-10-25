using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}