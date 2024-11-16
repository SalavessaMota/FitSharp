using FitSharp.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<Notification> GetNotificationByIdAsync(int id);

        IQueryable<Notification> GetNotifications(string userId);
    }
}