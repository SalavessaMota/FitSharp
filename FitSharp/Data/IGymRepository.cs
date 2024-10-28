using FitSharp.Data.Entities;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IGymRepository : IGenericRepository<Gym>
    {
        IQueryable GetGymsWithRooms();

        Task<Gym> GetGymWithRoomsAsync(int id);

        Task<Room> GetRoomAsync(int id);

        Task AddRoomAsync(RoomViewModel model);

        Task<int> UpdateRoomAsync(Room room);

        Task<int> DeleteRoomAsync(Room room);

        IEnumerable<SelectListItem> GetComboGyms();

        Task<IEnumerable<SelectListItem>> GetComboRoomsAsync(int gymId);

        Task<Gym> GetGymAsync(Room room);
    }
}
