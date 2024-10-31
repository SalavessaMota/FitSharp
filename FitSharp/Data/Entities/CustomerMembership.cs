using System;

namespace FitSharp.Data.Entities
{
    public class CustomerMembership : IEntity
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int MembershipId { get; set; }
        public Membership Membership { get; set; }

        public int ClassesRemaining { get; set; }

        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}