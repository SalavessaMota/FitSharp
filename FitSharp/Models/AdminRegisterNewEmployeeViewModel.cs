using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class AdminRegisterNewEmployeeViewModel : AdminRegisterNewUserViewModel
    {
        [Display(Name = "Gym")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a gym.")]
        public int GymId { get; set; }

        public IEnumerable<SelectListItem> Gyms { get; set; }
    }
}
