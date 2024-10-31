using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class PersonalClassesRepository : GenericRepository<PersonalClass>, IPersonalClassesRepository
    {
        private readonly DataContext _context;

        public PersonalClassesRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedData()
        {
            return _context.PersonalClasses
                .Include(p => p.Room)
                .Include(p => p.ClassType)
                .Include(p => p.Instructor)
                .Include(p => p.Customer);
        }

        public IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedDataByInstructorName(string instructorName)
        {
            return _context.PersonalClasses
                .Include(p => p.Room)
                .Include(p => p.ClassType)
                .Include(p => p.Instructor)
                .ThenInclude(i => i.User)
                .Include(p => p.Customer)
                .Where(p => p.Instructor.User.UserName == instructorName);
        }

    }
}
