using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FitSharp.Models
{
    public class CreatePersonalClassViewModel : PersonalClass
    {
        public IEnumerable<SelectListItem> Rooms { get; set; }

        public IEnumerable<SelectListItem> ClassTypes { get; set; }
    }
}