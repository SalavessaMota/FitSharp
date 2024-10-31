using FitSharp.Data.Entities;

namespace FitSharp.Data
{
    public class ClassTypeRepository : GenericRepository<ClassType>, IClassTypeRepository
    {
        private readonly DataContext _context;

        public ClassTypeRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
