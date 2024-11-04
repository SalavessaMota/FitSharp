using FitSharp.Entities;

namespace FitSharp.Data.Entities
{
    public class Customer : IEntity
    {
        public int Id { get; set; }

        public User User { get; set; }

        public int? MembershipId { get; set; }
        public virtual Membership Membership { get; set; }

        //public virtual ICollection<GroupClass> GroupClasses { get; set; }

        //public DateTime MembershipBeginDate { get; set; }
    }
}