using System.Collections.Generic;

namespace FitSharp.Data.Entities
{
    public class Instructor : Employee
    {
        public string Speciality { get; set; }

        public string Description { get; set; }

        public virtual ICollection<GroupClass> GroupClasses { get; set; }

        public virtual ICollection<PersonalClass> PersonalClasses { get; set; }
    }
}