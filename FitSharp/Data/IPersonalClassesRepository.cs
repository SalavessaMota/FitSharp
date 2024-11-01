using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IPersonalClassesRepository : IGenericRepository<PersonalClass>
    {
        Task<PersonalClass> GetPersonalClassWithAllRelatedData(int id);

        IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedData();

        IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedDataByUserName(string instructorName);
    }
}
