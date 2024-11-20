using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        [Display(Name = "Number of Rooms")]
        public int NumberOfRooms => Rooms == null ? 0 : Rooms.Count;

        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();

        [Display(Name = "Number of Equipments")]
        public int NumberOfEquipments => Equipments == null ? 0 : Equipments.Count;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public virtual ICollection<GymReview> Reviews { get; set; }

        public double Rating => (Reviews?.Count ?? 0) > 0 ? Reviews.Average(r => r.Stars) : 0;

        [Display(Name = "Image")]
        public Guid ImageId { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://aircinelmvc.blob.core.windows.net/resources/DefaultGymImage2.png"
            : $"https://aircinelmvc.blob.core.windows.net/gyms/{ImageId}";
    }
}