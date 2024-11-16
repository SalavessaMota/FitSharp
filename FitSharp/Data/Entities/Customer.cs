using FitSharp.Entities;
using System;
using System.Collections.Generic;

namespace FitSharp.Data.Entities
{
    public class Customer : IEntity
    {
        public int Id { get; set; }

        public User User { get; set; }

        public int? MembershipId { get; set; }
        public virtual Membership Membership { get; set; }

        public int ClassesRemaining { get; set; }

        public DateTime MembershipBeginDate { get; set; }
        public DateTime MembershipEndDate { get; set; }

        public bool MembershipIsActive { get; set; }

        // Relação com GroupClass
        public virtual ICollection<GroupClass> GroupClasses { get; set; } = new List<GroupClass>();

        // Relação com PersonalClass
        public virtual ICollection<PersonalClass> PersonalClasses { get; set; } = new List<PersonalClass>();
    }
}