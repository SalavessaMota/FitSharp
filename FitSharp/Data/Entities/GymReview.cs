using System.ComponentModel.DataAnnotations;
using System;

namespace FitSharp.Data.Entities
{
    public class GymReview : IEntity
    {
        public int Id { get; set; }

        [Range(1, 5)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Stars { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int GymId { get; set; }

        public Gym Gym { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}
