using System;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class Review : IEntity
    {
        public int Id { get; set; }

        [Range(1, 5)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Stars { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int InstructorId { get; set; }

        public Instructor Instructor { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}