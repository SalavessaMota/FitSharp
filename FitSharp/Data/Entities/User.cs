using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50, ErrorMessage = "The field {0} only can contain {1} characters length.")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The field {0} only can contain {1} characters length.")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "The field {0} only can contain {1} characters length.")]
        public string Address { get; set; }

        [Required]
        [MaxLength(15, ErrorMessage = "The field {0} only can contain {1} characters length.")]
        public string TaxNumber { get; set; }

        public int CityId { get; set; }

        public City City { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Image")]
        public Guid ImageId { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://aircinelmvc.blob.core.windows.net/resources/default_profile_picture.jpg"
            : $"https://aircinelmvc.blob.core.windows.net/users/{ImageId}";
    }
}