using System;

namespace FitSharp.Data.Dtos
{
    public class CustomerMembershipDto
    {
        public string Name { get; set; }

        public int ClassesRemaining { get; set; }

        public string Description { get; set; }

        public DateTime MembershipBeginDate { get; set; }

        public DateTime MembershipEndDate { get; set; }
    }
}
