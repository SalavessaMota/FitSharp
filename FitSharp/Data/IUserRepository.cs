using FitSharp.Data.Entities;
using FitSharp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IUserRepository
    {
        Task<object> GetEntityByUserIdAsync(string id);

        IQueryable<User> GetAllUsersWithCityAndCountry();

        Task<Customer> GetCustomerByUserName(string userName);

        Task<Employee> GetEmployeeByUserName(string userName);

        IQueryable<Customer> GetAllCustomersWithAllRelatedData();

        IEnumerable<Instructor> GetAllInstructorsWithAllRelatedData();

        IEnumerable<Employee> GetAllEmployeesWithAllRelatedData();

        IEnumerable<Admin> GetAllAdminsWithAllRelatedData();

        Task<Instructor> GetInstructorWithAllRelatedDataByInstructorIdAsync(int instructorId);

        Task AddCustomerAsync(Customer customer);

        Task AddEmployeeAsync(Employee employee);

        Task AddAdminAsync(Admin admin);

        Task<User> GetUserByIdAsync(string userId);

        Task<User> GetUserWithCountryAndCityByIdAsync(string userId);

        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<Customer> GetCustomerByIdAsync(int? customerId);

        Task<Instructor> GetInstructorByUserName(string instructorName);

        IEnumerable<SelectListItem> GetComboInstructors();

        Task AddToSpecificTableAsync(User user, string role);

        Task RemoveFromSpecificTableAsync(User user);

        string DetermineUserType(User user);

        Task UpdateCustomerAsync(Customer customer);
    }
}