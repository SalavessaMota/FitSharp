using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class ContactUsViewModel
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [MaxLength(50, ErrorMessage = "The field {0} can contain {1} characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [MaxLength(50, ErrorMessage = "The field {0} can contain {1} characters.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [StringLength(500, ErrorMessage = "The field {0} can contain {1} characters.")]
        public string Message { get; set; }
    }
}
