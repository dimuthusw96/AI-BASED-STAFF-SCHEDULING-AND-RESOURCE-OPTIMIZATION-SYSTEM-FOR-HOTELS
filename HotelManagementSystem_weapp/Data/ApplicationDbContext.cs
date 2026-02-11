
using HotelManagementSystem_weapp.Models;
using Microsoft.EntityFrameworkCore;
namespace HotelManagementSystem_weapp.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<Forecast> Forecasts { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        
    }
}
