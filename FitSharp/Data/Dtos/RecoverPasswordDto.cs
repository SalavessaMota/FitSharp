using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Dtos
{
    public class RecoverPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}