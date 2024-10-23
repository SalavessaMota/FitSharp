using FitSharp.Data.Entities;
using FitSharp.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitSharp.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Equipment> Equipment { get; set; }

        public DbSet<GroupClass> GroupClasses { get; set; }
        public DbSet<PersonalClass> PersonalClasses { get; set; }

        public DbSet<ClassTypes> ClassTypes { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Admin> Admins { get; set; }

        public DbSet<Membership> Memberships { get; set; }
        //public DbSet<Reservation> Reservations { get; set; }
        




        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {            
        }

        
    }
}
