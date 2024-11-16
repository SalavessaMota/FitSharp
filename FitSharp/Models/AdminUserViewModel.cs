using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class AdminUserViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Full Name")]
        public string Fullname { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Address { get; set; }

        public string CityName { get; set; }

        public string CountryName { get; set; }

        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }

        public string ImageFullPath { get; set; }
    }
}