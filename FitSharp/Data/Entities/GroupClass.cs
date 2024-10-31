using System.Collections.Generic;

namespace FitSharp.Data.Entities
{
    public class GroupClass : Session
    {
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}