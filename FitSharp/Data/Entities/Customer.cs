using FitSharp.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;

namespace FitSharp.Data.Entities
{
    public class Customer : IEntity
    {
        public int Id { get; set; }

        public User User { get; set; }


        public int? MembershipId { get; set; }
        public virtual Membership Membership { get; set; }
        
        public virtual ICollection<GroupClass> GroupClasses { get; set; }
    }
}
