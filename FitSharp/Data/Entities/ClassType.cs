namespace FitSharp.Data.Entities
{
    public class ClassType : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}