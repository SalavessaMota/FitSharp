using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class AdminRegisterNewAdminViewModel : UserViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
    }
}