using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class Equipment : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int GymId { get; set; }

        public virtual Gym Gym { get; set; }

        [Display(Name = "Requires Repair")]
        public bool RequiresRepair { get; set; }
    }
}