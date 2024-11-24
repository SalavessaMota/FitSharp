using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class AdminRegisterNewAdminViewModel : AdminRegisterNewUserViewModel
    {
        

        [Required]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
    }
}