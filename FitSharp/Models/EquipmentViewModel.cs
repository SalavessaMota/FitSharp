using FitSharp.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class EquipmentViewModel
    {
        public int GymId { get; set; }

        public int EquipmentId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The field {0} can contain {1} characters.")]
        public string Name { get; set; }

        public string Description { get; set; }

    }
}
