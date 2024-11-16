using FitSharp.Data.Entities;
using FitSharp.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserRepository userRepository, IUserHelper userHelper)
        {
            _context = context;
            _userRepository = userRepository;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Employee");
            await _userHelper.CheckRoleAsync("Instructor");
            await _userHelper.CheckRoleAsync("Customer");

            // COUNTRIES AND CITIES SEEDING
            if (!_context.Countries.Any())
            {
                var cities = new List<City>();
                cities.Add(new City { Name = "Lisboa" });
                cities.Add(new City { Name = "Porto" });
                cities.Add(new City { Name = "Faro" });

                _context.Countries.Add(new Country
                {
                    Cities = cities,
                    Name = "Portugal",
                    Code = "PT"
                });

                await _context.SaveChangesAsync();
            }

            // GYM SEEDING
            var lisboa = _context.Cities.FirstOrDefault(c => c.Name == "Lisboa" && c.Country.Code == "PT");
            if (lisboa != null && !_context.Gyms.Any(g => g.CityId == lisboa.Id && g.Name == "Saldanha FitSharp"))
            {
                var gym = new Gym
                {
                    Name = "Saldanha FitSharp",
                    Address = "Avenida da República, Saldanha",
                    CityId = lisboa.Id,

                    Rooms = new List<Room>
                    {
                        new Room { Name = "Main Hall", Capacity = 50 }
                    },

                    Equipments = new List<Equipment>
                    {
                        new Equipment { Name = "Treadmill", Description = "Treadmill for running", RequiresRepair = false }
                    }
                };

                _context.Gyms.Add(gym);
                await _context.SaveChangesAsync();
            }

            // ADMIN SEEDING
            var adminUser = await _userRepository.GetUserByEmailAsync("nunosalavessa@hotmail.com");

            if (adminUser == null)
            {
                adminUser = new User
                {
                    FirstName = "Nuno",
                    LastName = "Salavessa",
                    Email = "nunosalavessa@hotmail.com",
                    UserName = "nunosalavessa@hotmail.com",
                    PhoneNumber = "212343555",
                    Address = "Rua Jau 33",
                    CityId = lisboa.Id,
                    City = lisboa,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(adminUser, "123123");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the admin user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(adminUser, "Admin");
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(adminUser);
                await _userHelper.ConfirmEmailAsync(adminUser, token);

                var admin = new Admin { User = adminUser };
                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(adminUser, "Admin");
            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(adminUser, "Admin");
            }

            // INSTRUCTOR SEEDING
            var ptUser = await _userRepository.GetUserByEmailAsync("tiagomonteirinho@yopmail.com");

            if (ptUser == null)
            {
                ptUser = new User
                {
                    FirstName = "Tiago",
                    LastName = "Monteirinho",
                    Email = "tiagomonteirinho@yopmail.com",
                    UserName = "tiagomonteirinho@yopmail.com",
                    PhoneNumber = "212343555",
                    Address = "Rua Jau 33",
                    CityId = lisboa.Id,
                    City = lisboa,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(ptUser, "123123");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the Instructor user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(ptUser, "Instructor");
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(ptUser);
                await _userHelper.ConfirmEmailAsync(ptUser, token);

                var instructor = new Instructor
                {
                    User = ptUser,
                    GymId = _context.Gyms.FirstOrDefault().Id,
                    Speciality = "Body Building",
                    Description = "I'm a body building instructor with 10 years of experience"
                };

                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
            }

            var isPtInRole = await _userHelper.IsUserInRoleAsync(ptUser, "Instructor");
            if (!isPtInRole)
            {
                await _userHelper.AddUserToRoleAsync(ptUser, "Instructor");
            }

            // EMPLOYEE SEEDING
            var employeeUser = await _userRepository.GetUserByEmailAsync("ritamiguens@yopmail.com");

            if (employeeUser == null)
            {
                employeeUser = new User
                {
                    FirstName = "Rita",
                    LastName = "Miguens",
                    Email = "ritamiguens@yopmail.com",
                    UserName = "ritamiguens@yopmail.com",
                    PhoneNumber = "212343555",
                    Address = "Rua Jau 33",
                    CityId = lisboa.Id,
                    City = lisboa,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(employeeUser, "123123");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the Employee user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(employeeUser, "Employee");
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(employeeUser);
                await _userHelper.ConfirmEmailAsync(employeeUser, token);

                var employee = new Employee
                {
                    User = employeeUser,
                    GymId = _context.Gyms.FirstOrDefault().Id
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
            }

            var isEmployeeInRole = await _userHelper.IsUserInRoleAsync(employeeUser, "Employee");
            if (!isEmployeeInRole)
            {
                await _userHelper.AddUserToRoleAsync(employeeUser, "Employee");
            }

            // MEMBERSHIPS SEEDING
            if (!_context.Memberships.Any())
            {
                _context.Memberships.Add(new Membership
                {
                    Name = "Basic",
                    Price = 40.00m,
                    NumberOfClasses = 8,
                    Description = "Basic Plan: Access to 8 classes per month and unlimited use of gym equipment."
                });

                _context.Memberships.Add(new Membership
                {
                    Name = "Premium",
                    Price = 70.00m,
                    NumberOfClasses = 16,
                    Description = "Premium Plan: Access to 16 classes per month and unlimited use of gym equipment, ideal for a more intense routine."
                });

                _context.Memberships.Add(new Membership
                {
                    Name = "Ultimate",
                    Price = 90.00m,
                    NumberOfClasses = 999999999,
                    Description = "Ultimate Plan: Unlimited classes and unlimited use of gym equipment, perfect for clients seeking maximum flexibility and personalization."
                });

                _context.Memberships.Add(new Membership
                {
                    Name = "Trial",
                    Price = 0.00m,
                    NumberOfClasses = 2,
                    Description = "Trial Membership: Access to 2 classes over a 1-week period."
                });

                await _context.SaveChangesAsync();
            }

            // CUSTOMER SEEDING
            var customerUser = await _userRepository.GetUserByEmailAsync("customerfitsharp@yopmail.com");

            if (customerUser == null)
            {
                customerUser = new User
                {
                    FirstName = "Customer",
                    LastName = "Inicial",
                    Email = "customerfitsharp@yopmail.com",
                    UserName = "customerfitsharp@yopmail.com",
                    PhoneNumber = "212343555",
                    Address = "Rua Jau 33",
                    CityId = lisboa.Id,
                    City = lisboa,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(customerUser, "123123");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the Customer user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(customerUser, "Customer");
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(customerUser);
                await _userHelper.ConfirmEmailAsync(customerUser, token);

                var customer = new Customer
                {
                    User = customerUser,
                    MembershipIsActive = true,
                    MembershipBeginDate = DateTime.Now,
                    MembershipEndDate = DateTime.Now.AddMonths(1),
                    ClassesRemaining = 2,
                    MembershipId = 1
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            var isCustomerInRole = await _userHelper.IsUserInRoleAsync(customerUser, "Customer");
            if (!isCustomerInRole)
            {
                await _userHelper.AddUserToRoleAsync(customerUser, "Customer");
            }

            // CLASSTYPES SEEDING
            if (!_context.ClassTypes.Any())
            {
                _context.ClassTypes.Add(new ClassType
                {
                    Name = "Personal Training",
                    Description = "One to one training with a personal trainer"
                });
                await _context.SaveChangesAsync();
            }

        }
    }
}