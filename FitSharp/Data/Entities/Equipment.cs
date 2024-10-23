namespace FitSharp.Data.Entities
{
    public class Equipment : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int GymId { get; set; }

        public virtual Gym Gym { get; set; }

        public bool RequiresRepair { get; set; }
    }
}
