using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem_weapp.Data;
using HotelManagementSystem_weapp.Models;
namespace HotelManagementSystem.Authtest
{
    public class AuthServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Login_Should_Succeed_With_Valid_Credentials()
        {
            var db = GetDbContext();

            var user = new ApplicationUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                FullName = "Admin",
                Role = "Admin"
            };

            db.ApplicationUsers.Add(user);
            await db.SaveChangesAsync();

            var service = new AuthService(db);

            var result = await service.LoginAsync("admin", "1234");

            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Not_Found()
        {
            var db = GetDbContext();
            var service = new AuthService(db);

            var result = await service.LoginAsync("unknown", "1234");

            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Login_Should_Fail_When_Password_Wrong()
        {
            var db = GetDbContext();

            db.ApplicationUsers.Add(new ApplicationUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234")
            });

            await db.SaveChangesAsync();

            var service = new AuthService(db);

            var result = await service.LoginAsync("admin", "wrong");

            result.Success.Should().BeFalse();
        }
    }
}