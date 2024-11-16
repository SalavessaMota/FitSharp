using FitSharp.Data.Entities;
using System.Collections.Generic;

namespace FitSharp.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Gym> Gyms { get; set; }

        public IEnumerable<Instructor> Instructors { get; set; }
    }
}