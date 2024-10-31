using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IPersonalClassesRepository : IGenericRepository<PersonalClass>
    {
        IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedData();

        IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedDataByInstructorName(string instructorName);
    }
}
