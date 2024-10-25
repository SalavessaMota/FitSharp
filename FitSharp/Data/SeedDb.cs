using FitSharp.Data.Entities;
using FitSharp.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Identity;
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
            await _context.Database.EnsureCreatedAsync();

            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Employee");
            await _userHelper.CheckRoleAsync("Customer");

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

            var user = await _userRepository.GetUserByEmailAsync("nunosalavessa@hotmail.com");

            if (user == null)
            {
                user = new User
                {
                    FirstName = "Nuno",
                    LastName = "Salavessa",
                    Email = "nunosalavessa@hotmail.com",
                    UserName = "nunosalavessa@hotmail.com",
                    PhoneNumber = "212343555",
                    Address = "Rua Jau 33",
                    CityId = _context.Countries.FirstOrDefault().Cities.FirstOrDefault().Id,
                    City = _context.Countries.FirstOrDefault().Cities.FirstOrDefault()
                };

                var result = await _userRepository.AddUserAsync(user, "123123");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin");
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var admin = new Admin { User = user };
                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, "Admin");
            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
        }
    }
}