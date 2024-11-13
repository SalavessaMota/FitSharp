using System.Collections.Generic;
using System.Linq;

namespace FitSharp.Data.Entities
{
    public class Instructor : Employee
    {
        public string Speciality { get; set; }

        public string Description { get; set; }

        public virtual ICollection<GroupClass> GroupClasses { get; set; }

        public virtual ICollection<PersonalClass> PersonalClasses { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        public double Rating => (Reviews?.Count ?? 0) > 0 ? Reviews.Average(r => r.Stars) : 0;
    }
}