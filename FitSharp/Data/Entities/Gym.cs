using System.Collections.Generic;

namespace FitSharp.Data.Entities
{
    public class Gym : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int CityId { get; set; }

        public virtual City City { get; set; }

        public ICollection<Room> Rooms { get; set; }

        public int NumberOfRooms => Rooms == null ? 0 : Rooms.Count;

        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();

        public int NumberOfEquipments => Equipments == null ? 0 : Equipments.Count;

        //TODO: Image collection.
    }
}