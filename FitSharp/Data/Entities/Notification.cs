using FitSharp.Entities;
using System;

namespace FitSharp.Data.Entities
{
    public class Notification : IEntity
    {
        public Notification()
        {
            Date = DateTime.Now;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Action { get; set; }

        public User User { get; set; }

        public string UserId { get; set; }

        public string Role { get; set; }

        public DateTime Date { get; set; }

        public bool IsRead { get; set; }
    }
}

//TODO: Implement payment methods;