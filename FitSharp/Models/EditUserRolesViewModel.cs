using System.Collections.Generic;

namespace FitSharp.Models
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<UserRoleViewModel> Roles { get; set; }
        public string SelectedRole { get; set; }
        public string Address { get; set; }
        public string CityName { get; set; }

        public string CountryName { get; set; }
        public string ImageFullPath { get; set; }
    }
}