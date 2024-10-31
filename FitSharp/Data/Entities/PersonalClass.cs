namespace FitSharp.Data.Entities
{
    public class PersonalClass : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public int ClassTypeId { get; set; }
        public virtual ClassType ClassType { get; set; }

        public int InstructorId { get; set; }
        public virtual Instructor Instructor { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}