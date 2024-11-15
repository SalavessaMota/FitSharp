using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task AddRoomAsync(Room room)
        {
            var gym = await this.GetGymWithRoomsAsync(room.GymId);
            if (gym == null)
            {
                return;
            }

            gym.Rooms.Add(room);
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

        public async Task<int> DeleteEquipmentAsync(Equipment equipment)
        {
            var gym = await _context.Gyms
                .Where(c => c.Equipments.Any(e => e.Id == equipment.Id))
                .FirstOrDefaultAsync();
            if (gym == null)
            {
                return 0;
            }

            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync();
            return gym.Id;
        }

        public async Task AddEquipmentAsync(Equipment equipment)
        {
            var gym = await this.GetGymWithEquipmentsAsync(equipment.GymId);
            if (gym == null)
            {
                return;
            }

            gym.Equipments.Add(equipment);
            _context.Gyms.Update(gym);
            await _context.SaveChangesAsync();
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

        public async Task<Gym> GetGymByRoomAsync(Room room)
        {
            return await _context.Gyms
                .Where(g => g.Rooms.Any(r => r.Id == room.Id))
                .FirstOrDefaultAsync();
        }

        public async Task<Room> GetRoomAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task<Equipment> GetEquipmentAsync(int id)
        {
            return await _context.Equipments.FindAsync(id);
        }

        public async Task<IEnumerable<Gym>> GetGymsWithRoomsAndEquipmentsAsync()
        {
            return await _context.Gyms
                .Include(g => g.Rooms) // Inclui as salas para contar o número de salas
                .Include(g => g.Equipments) // Inclui os equipamentos para contar o número de equipamentos
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Gym> GetGymWithAllRelatedDataAsync(int id)
        {
            return await _context.Gyms
                .Include(g => g.City)
                .ThenInclude(g => g.Country)
                .Include(g => g.Rooms)
                .Include(g => g.Equipments)
                .Include(g => g.Employees)
                .Where(g => g.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Gym> GetGymWithRoomsAsync(int id)
        {
            return await _context.Gyms
                .Include(g => g.Rooms)
                .Where(g => g.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Gym> GetGymWithEquipmentsAsync(int id)
        {
            return await _context.Gyms
                .Include(g => g.Equipments)
                .Where(g => g.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Gym> GetGymWithRoomsAndEquipmentsAsync(int id)
        {
            return await _context.Gyms
                .Include(g => g.Rooms)
                .Include(g => g.Equipments)
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

        public async Task<int> UpdateEquipmentAsync(Equipment equipment)
        {
            var gym = await _context.Gyms
                .Where(g => g.Id == equipment.GymId)
                .FirstOrDefaultAsync();

            if (gym == null)
            {
                throw new Exception("The specified gym does not exist.");
            }

            _context.Equipments.Update(equipment);
            await _context.SaveChangesAsync();
            return gym.Id;
        }

        public IEnumerable<SelectListItem> GetComboRoomsByInstructorName(string instructorName)
        {
            var rooms = _context.Rooms
                .Include(r => r.Gym)
                .ThenInclude(g => g.Employees)
                .Where(r => r.Gym.Employees.Any(e => e.User.UserName == instructorName))
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                })
                .OrderBy(r => r.Text)
                .ToList();

            rooms.Insert(0, new SelectListItem
            {
                Text = "(Select a room...)",
                Value = string.Empty
            });

            return rooms;
        }

        public async Task<IEnumerable<Gym>> GetGymsWithAllRelatedDataAsync()
        {
            return await _context.Gyms
                .Include(g => g.City)
                .ThenInclude(g => g.Country)
                .Include(g => g.Rooms)
                .Include(g => g.Equipments)
                .Include(g => g.Employees)
                .ToListAsync();
        }

        public async Task<IEnumerable<Equipment>> GetEquipments()
        {
            return await _context.Equipments
                .Include(e => e.Gym)
                .ToListAsync();
        }

        public async Task<IEnumerable<Equipment>> GetGymEquipments(int? gymId)
        {
            return await _context.Equipments
                .Include(e => e.Gym)
                .Where(e => e.Gym.Id == gymId)
                .ToListAsync();
        }
    }
}