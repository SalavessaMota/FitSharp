using FitSharp.Data.Entities;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class GymRepository : GenericRepository<Gym>, IGymRepository
    {
        private readonly DataContext _context;

        public GymRepository(DataContext context) : base(context)
        {
            _context = context;
        }
        public async Task AddRoomAsync(RoomViewModel model)
        {
            var gym = await this.GetGymWithRoomsAsync(model.GymId);
            if (gym == null)
            {
                return;
            }

            gym.Rooms.Add(new Room { Name = model.Name });
            _context.Gyms.Update(gym);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteRoomAsync(Room room)
        {
            var gym = await _context.Gyms
                .Where(c => c.Rooms.Any(r => r.Id == room.Id))
                .FirstOrDefaultAsync();
            if (gym == null)
            {
                return 0;
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return gym.Id;
        }

        public IEnumerable<SelectListItem> GetComboGyms()
        {
            var list = _context.Gyms.Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            }).OrderBy(sli => sli.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a gym...)",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboRoomsAsync(int gymId)
        {
            var gym = await _context.Gyms.Include(g => g.Rooms)
                                  .FirstOrDefaultAsync(g => g.Id == gymId);
            var list = new List<SelectListItem>();
            if (gym != null)
            {
                list = gym.Rooms.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).OrderBy(sli => sli.Text).ToList();

                list.Insert(0, new SelectListItem
                {
                    Text = "(Select a room...)",
                    Value = "0"
                });
            }

            return list;
        }

        public async Task<Gym> GetGymAsync(Room room)
        {
            return await _context.Gyms
                .Where(g => g.Rooms.Any(r => r.Id == room.Id))
                .FirstOrDefaultAsync();
        }

        public async Task<Room> GetRoomAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public IQueryable GetGymsWithRooms()
        {
            return _context.Gyms
                .Include(g => g.Rooms)
                .OrderBy(g => g.Name);
        }

        public async Task<Gym> GetGymWithRoomsAsync(int id)
        {
            return await _context.Gyms
                .Include(g => g.Rooms)
                .Where(g => g.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateRoomAsync(Room room)
        {
            var gym = await _context.Gyms
                .Where(g => g.Rooms.Any(r => r.Id == room.Id))
                .FirstOrDefaultAsync();

            if (gym == null)
            {
                return 0;
            }

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return gym.Id;
        }
    }
}
