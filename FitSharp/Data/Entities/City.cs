using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class City : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CountryId { get; set; }

        public Country Country { get; set; }

        public ICollection<Gym> Gyms { get; set; } = new List<Gym>();

        [Display(Name = "Number of Gyms")]
        public int NumberOfGyms => Gyms == null ? 0 : Gyms.Count;
    }
}