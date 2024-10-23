namespace FitSharp.Data.Entities
{
    public class Room : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Capacity { get; set; }

        public bool IsVirtual { get; set; }

        public int GymId { get; set; }

        public virtual Gym Gym { get; set; }
    }
}