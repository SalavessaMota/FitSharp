using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class Membership : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        [Display(Name = "Max Classes Per Month")]
        public int NumberOfClasses{ get; set; }

        public ICollection<Customer> Customers { get; set; }
    }
}