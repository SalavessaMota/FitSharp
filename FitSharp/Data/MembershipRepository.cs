using FitSharp.Data.Entities;

namespace FitSharp.Data
{
    public class MembershipRepository : GenericRepository<Membership>, IMembershipRepository
    {
        private readonly DataContext _context;

        public MembershipRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
