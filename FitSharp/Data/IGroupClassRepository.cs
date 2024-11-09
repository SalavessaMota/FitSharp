using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IGroupClassRepository : IGenericRepository<GroupClass>
    {
        Task<GroupClass> GetGroupClassWithAllRelatedDataAsync(int id);

        Task<IEnumerable<GroupClass>> GetGroupClassesWithAllRelatedDataAsync();
    }
}