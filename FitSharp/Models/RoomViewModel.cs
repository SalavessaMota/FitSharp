using System.ComponentModel.DataAnnotations;

namespace FitSharp.Models
{
    public class RoomViewModel
    {
        public int GymId { get; set; }

        public int RoomId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The field {0} can contain {1} characters.")]
        public string Name { get; set; }

        [Range(1, 50)]
        public int Capacity { get; set; }

        public bool IsVirtual { get; set; }
    }
}