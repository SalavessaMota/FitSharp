using FitSharp.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IPersonalClassRepository : IGenericRepository<PersonalClass>
    {
        Task<PersonalClass> GetPersonalClassWithAllRelatedData(int id);

        IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedData();

        IQueryable<PersonalClass> GetAllPersonalClassesWithRelatedDataByUserName(string instructorName);

        Task<bool> HasAttendedGymAsync(int customerId, int gymId);

        Task<bool> HasAttendedInstructorAsync(int customerId, int instructorId);


    }
}