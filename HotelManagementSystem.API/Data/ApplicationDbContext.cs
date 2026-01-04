
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.API_.Models;
namespace HotelManagementSystem.API_.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Mapping our Tables
        public DbSet<Forecast> Forecasts { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Shift> Shifts { get; set; }
    }
}
