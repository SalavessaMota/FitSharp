namespace FitSharp.Data.Entities
{
    public class PersonalClass : Session
    {    
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}