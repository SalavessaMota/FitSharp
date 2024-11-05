using FitSharp.Data.Entities;
using FitSharp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public UserRepository(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IQueryable<User> GetAllUsersWithCityAndCountry()
        {
            return _context.Users
             .Include(u => u.City)
             .ThenInclude(c => c.Country);
        }

        public IQueryable<Customer> GetAllCustomersWithUser()
        {
            return _context.Customers
                .Include(c => c.User);
        }

        public async Task<Customer> GetCustomerByUserIdAsync(string id)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Include(c => c.User.City)
                .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(c => c.User.Id == id);
        }

        public async Task<Employee> GetEmployeeByUserIdAsync(string id)
        {
            return await _context.Employees
                .Include(e => e.User)
                .Include(e => e.User.City)
                .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(e => e.User.Id == id);
        }

        public async Task<Admin> GetAdminByUserIdAsync(string id)
        {
            return await _context.Admins
                .Include(a => a.User)
                .Include(a => a.User.City)
                .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(a => a.User.Id == id);
        }

        public Task<bool> IsCustomerAsync(User user)
        {
            return _context.Customers.AnyAsync(c => c.User.Id == user.Id);
        }

        public Task<bool> IsEmployeeAsync(User user)
        {
            return _context.Employees.AnyAsync(e => e.User.Id == user.Id);
        }

        public Task<bool> IsAdminAsync(User user)
        {
            return _context.Admins.AnyAsync(a => a.User.Id == user.Id);
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task AddInstructorAsync(Instructor instructor)
        {
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();
        }

        public async Task AddAdminAsync(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
        }

        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task DeleteCustomerAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(Employee employee)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInstructorAsync(Instructor instructor)
        {
            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAdminAsync(Admin admin)
        {
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<User> GetUserWithCountryAndCityByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers
                .Include(c => c.User)
                .FirstAsync(c => c.Id == customerId);
            //.FirstOrDefault(c => c.Id == customerId);
        }

        public Instructor GetInstructorByUserName(string instructorName)
        {
            return _context.Instructors
                .Include(i => i.User)
                .FirstOrDefault(i => i.User.UserName == instructorName);
        }

        public IEnumerable<SelectListItem> GetComboInstructors()
        {
            var instructors = _context.Instructors
                .Include(i => i.User)
                .Select(i => new SelectListItem
                {
                    Text = i.User.FullName,
                    Value = i.Id.ToString()
                })
                .ToList();

            instructors.Insert(0, new SelectListItem
            {
                Text = "(Select an instructor...)",
                Value = "0"
            });

            return instructors;
        }
    }
}