using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class GroupClassRepository : GenericRepository<GroupClass>, IGroupClassRepository
    {
        private readonly DataContext _context;

        public GroupClassRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<GroupClass> GetGroupClassWithAllRelatedDataAsync(int id)
        {
            return await _context.GroupClasses
                .Include(g => g.Room)
                .ThenInclude(r => r.Gym)
                .Include(g => g.ClassType)
                .Include(g => g.Instructor)
                .ThenInclude(i => i.User)
                .Include(g => g.Customers)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<GroupClass>> GetGroupClassesWithAllRelatedDataAsync()
        {
            return await _context.GroupClasses
                .Include(g => g.Room)
                .ThenInclude(r => r.Gym)
                .Include(g => g.ClassType)
                .Include(g => g.Instructor)
                .ThenInclude(i => i.User)
                .Include(g => g.Customers)
                .ThenInclude(c => c.User)
                .ToListAsync();
        }

        public IQueryable<GroupClass> GetAllGroupClassesWithRelatedDataByUserName(string userName)
        {
            return _context.GroupClasses
                .Include(g => g.Room)
                    .ThenInclude(r => r.Gym)
                .Include(g => g.ClassType)
                .Include(g => g.Instructor)
                    .ThenInclude(i => i.User)
                .Include(g => g.Customers)
                    .ThenInclude(c => c.User)
                .Where(g => g.Instructor.User.UserName == userName || g.Customers.Any(c => c.User.UserName == userName));
        }
    }
}