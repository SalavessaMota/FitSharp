using System.ComponentModel.DataAnnotations;

namespace FitSharp.Data.Entities
{
    public class PersonalClass : Session
    {
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}