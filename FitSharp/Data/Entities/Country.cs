using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class Country : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public ICollection<City> Cities { get; set; }

        [Display(Name = "Number of cities")]
        public int NumberCities => Cities == null ? 0 : Cities.Count;
    }
}