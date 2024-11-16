using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly DataContext _context;

        public NotificationRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public IQueryable<Notification> GetNotifications(string userId)
        {
            return _context.Notifications
                        .Include(n => n.User)
                        .Where(n => n.UserId == userId)
                        .OrderByDescending(n => n.Date);
        }
    }
}