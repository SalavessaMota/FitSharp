using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FitSharp.Models
{
    public class EditGroupClassViewModel : GroupClass
    {
        public IEnumerable<SelectListItem> Rooms { get; set; }

        public IEnumerable<SelectListItem> ClassTypes { get; set; }

        public IEnumerable<SelectListItem> Instructors { get; set; }

        public IEnumerable<SelectListItem> Customers { get; set; }
    }
}
