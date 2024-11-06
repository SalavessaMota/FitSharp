using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace FitSharp.Models
{
    public class AdminEditUserViewModel : UserViewModel
    {
        // Propriedade para identificar o tipo do usuário (Admin, Customer, Employee, Instructor)
        public string UserType { get; set; }

        // Propriedades específicas para Customer
        public int? MembershipId { get; set; }
        public IEnumerable<SelectListItem> Memberships { get; set; }

        // Propriedades específicas para Employee
        public int? GymId { get; set; }
        public IEnumerable<SelectListItem> Gyms { get; set; }

        // Propriedades específicas para Instructor (herda GymId de Employee)
        public string Speciality { get; set; }
        public string Description { get; set; }
    }
}
