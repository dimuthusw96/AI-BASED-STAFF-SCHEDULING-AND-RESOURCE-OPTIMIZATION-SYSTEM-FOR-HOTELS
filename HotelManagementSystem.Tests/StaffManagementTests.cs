using Bunit;
using HotelManagementSystem_weapp.Components.Pages;
using HotelManagementSystem_weapp.Data;
using HotelManagementSystem_weapp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
namespace HotelManagementSystem.staffmangement
{
    public class StaffManagementTests : TestContext
    {
        private IDbContextFactory<ApplicationDbContext> CreateFactory()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var factoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            factoryMock.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(context);

            return factoryMock.Object;
        }

        [Fact]
        public void Page_Loads_And_Shows_Header()
        {
            // Arrange
            Services.AddSingleton(CreateFactory());

            // Act
            var component = Render<Staff_management>(); // <-- USE Render()

            // Assert
            component.Markup.Contains("Staff Directory");
        }
        [Fact]
        public void Add_Button_Should_Open_Modal()
        {
            Services.AddSingleton(CreateFactory());

            var component = Render<Staff_management>();

            var button = component.Find("button.btn-primary");
            button.Click();

            component.Markup.Contains("Register New Staff");
        }
        [Fact]
        public async Task Should_Display_Existing_Staff()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            context.Staff.Add(new Staff
            {
                FullName = "John Doe",
                Role = "FrontDesk",
                HourlyRate = 15,
                MaxHoursPerWeek = 40,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var factoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            factoryMock.Setup(f => f.CreateDbContextAsync(default))
                       .ReturnsAsync(context);

            Services.AddSingleton(factoryMock.Object);

            var component = Render<Staff_management>();

            component.Markup.Contains("John Doe");
        }
    }

}