using FitSharp.Entities;

namespace FitSharp.Data.Entities
{
    public class Employee : IEntity
    {
        public int Id { get; set; }

        public User user { get; set; }

        public int? GymId { get; set; }

        public virtual Gym Gym { get; set; }
    }
}