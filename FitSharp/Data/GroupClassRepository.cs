using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class GroupClassRepository : GenericRepository<GroupClass>, IGroupClassRepository
    {
        private readonly DataContext _dataContext;

        public GroupClassRepository(DataContext dataContext) : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<GroupClass> GetGroupClassWithAllRelatedDataAsync(int id)
        {
            return await _dataContext.GroupClasses
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
            return await _dataContext.GroupClasses
                .Include(g => g.Room)
                .ThenInclude(r => r.Gym)
                .Include(g => g.ClassType)
                .Include(g => g.Instructor)
                .ThenInclude(i => i.User)
                .Include(g => g.Customers)
                .ThenInclude(c => c.User)
                .ToListAsync();
        }
    }
}