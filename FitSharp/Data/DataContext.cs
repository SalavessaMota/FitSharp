using FitSharp.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitSharp.Data
{
    public class DataContext : IdentityDbContext<User>
    {


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {            
        }

        
    }
}
