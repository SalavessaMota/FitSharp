using System;
using System.Collections.Generic;

namespace FitSharp.Data.Entities
{
    public class Membership
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public int MaxClassesPerMonth { get; set; }

        public ICollection<Customer> Customers { get; set; }
    }
}