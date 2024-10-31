using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IGymRepository : IGenericRepository<Gym>
    {
        IQueryable GetGymsWithRoomsAndEquipments();

        Task<Gym> GetGymWithRoomsAsync(int id);

        Task<Gym> GetGymWithEquipmentsAsync(int id);

        Task<Gym> GetGymWithRoomsAndEquipmentsAsync(int id);

        Task<Room> GetRoomAsync(int id);

        Task<Equipment> GetEquipmentAsync(int id);

        Task AddRoomAsync(Room room);

        Task AddEquipmentAsync(Equipment equipment);

        Task<int> UpdateRoomAsync(Room room);

        Task<int> UpdateEquipmentAsync(Equipment equipment);

        Task<int> DeleteRoomAsync(Room room);

        Task<int> DeleteEquipmentAsync(Equipment equipment);

        IEnumerable<SelectListItem> GetComboGyms();

        Task<IEnumerable<SelectListItem>> GetComboRoomsAsync(int gymId);

        Task<Gym> GetGymAsync(Room room);

        IEnumerable<SelectListItem> GetComboRoomsByInstructorName(string instructorName);
    }
}
