using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
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

        public IQueryable<GroupClass> GetAllGroupClassesWithRelatedData()
        {
            return _context.GroupClasses
                .Include(g => g.Room)
                .ThenInclude(r => r.Gym)
                .Include(g => g.ClassType)
                .Include(g => g.Instructor)
                .ThenInclude(i => i.User)
                .Include(g => g.Instructor)
                .ThenInclude(i => i.Reviews)
                .Include(g => g.Customers)
                .ThenInclude(c => c.User);
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
                .Where(g => g.Customers.Any(c => c.User.UserName == userName));
        }

        public async Task<bool> HasAttendedGymAsync(int customerId, int gymId)
        {
            return await _context.GroupClasses.AnyAsync(gc => gc.Customers.Any(c => c.Id == customerId) && gc.Room.GymId == gymId);
        }

        public async Task<bool> HasAttendedInstructorAsync(int customerId, int instructorId)
        {
            return await _context.GroupClasses.AnyAsync(gc => gc.Customers.Any(c => c.Id == customerId) && gc.InstructorId == instructorId);
        }

    }
}