using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class PersonalClassRepository : GenericRepository<PersonalClass>, IPersonalClassRepository
    {
        private readonly DataContext _context;

        public PersonalClassRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PersonalClass> GetPersonalClassWithAllRelatedData(int id)
        {
            return await _context.PersonalClasses
                .Include(p => p.Room)
                .ThenInclude(r => r.Gym)
                .Include(p => p.ClassType)
                .Include(p => p.Instructor)
                .ThenInclude(i => i.User)
                .Include(p => p.Customer)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedData()
        {
            return _context.PersonalClasses
                .Include(p => p.Room)
                .ThenInclude(r => r.Gym)
                .Include(p => p.ClassType)
                .Include(p => p.Instructor)
                .ThenInclude(i => i.User)
                .Include(p => p.Instructor)
                .ThenInclude(i => i.Reviews)
                .Include(p => p.Customer)
                .ThenInclude(c => c.User);
        }

        public IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedDataByUserName(string name)
        {
            return _context.PersonalClasses
                .Include(p => p.Room)
                    .ThenInclude(r => r.Gym)
                        .ThenInclude(g => g.Reviews)
                .Include(p => p.ClassType)
                .Include(p => p.Instructor)
                    .ThenInclude(i => i.User)
                .Include(p => p.Instructor)
                    .ThenInclude(i => i.Reviews)
                        .ThenInclude(r => r.Customer)
                .Include(p => p.Customer)
                    .ThenInclude(c => c.User)
                .Where(p => p.Customer.User.UserName == name);
        }
    }
}