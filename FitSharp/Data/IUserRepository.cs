using FitSharp.Data.Entities;
using FitSharp.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IUserRepository
    {
        IQueryable<User> GetAllUsersWithCityAndCountry();

        Task<Customer> GetCustomerByUserIdAsync(string id);

        Task<Employee> GetEmployeeByUserIdAsync(string id);

        Task<Admin> GetAdminByUserIdAsync(string id);

        Task<bool> IsCustomerAsync(User user);

        Task<bool> IsEmployeeAsync(User user);

        Task<bool> IsAdminAsync(User user);

        //IQueryable<User> GetAllUsers();
        Task AddCustomerAsync(Customer customer);

        Task AddEmployeeAsync(Employee employee);

        Task AddInstructorAsync(Instructor instructor);

        Task AddAdminAsync(Admin admin);

        Task<User> GetUserByIdAsync(string userId);

        Task<User> GetUserWithCountryAndCityByIdAsync(string userId);

        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> DeleteUserAsync(User user);

        Task DeleteCustomerAsync(Customer customer);

        Task DeleteEmployeeAsync(Employee employee);

        Task DeleteInstructorAsync(Instructor instructor);

        Task DeleteAdminAsync(Admin admin);
    }
}