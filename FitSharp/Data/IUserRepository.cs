using FitSharp.Data.Entities;
using FitSharp.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IUserRepository
    {
        //IQueryable<User> GetAllUsers();
        Task AddCustomerAsync(Customer customer);

        Task<User> GetUserByIdAsync(string userId);

        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> DeleteUserAsync(User user);
    }
}