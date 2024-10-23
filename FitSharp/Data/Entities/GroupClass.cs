using System.Collections;
using System.Collections.Generic;

namespace FitSharp.Data.Entities
{
    public class GroupClass : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public int ClassTypeId { get; set; }
        public virtual ClassTypes ClassType { get; set; }

        public int InstructorId { get; set; }
        public virtual Instructor Instructor { get; set; }

        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
