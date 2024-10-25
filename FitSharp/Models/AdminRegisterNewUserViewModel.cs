using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class AdminRegisterNewUserViewModel : UserViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
    }
}