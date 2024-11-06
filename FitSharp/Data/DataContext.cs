using FitSharp.Data.Entities;
using FitSharp.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FitSharp.Data
{
    //TODO: Change entity cascade delete at DataContext
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Equipment> Equipments { get; set; }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<GroupClass> GroupClasses { get; set; }
        public DbSet<PersonalClass> PersonalClasses { get; set; }

        public DbSet<ClassType> ClassTypes { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Instructor> Instructors { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<City>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Gym>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(p => p.Cities)
                .HasForeignKey(c => c.CountryId);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}