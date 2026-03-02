using Bunit;
using HotelManagementSystem_weapp.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Radzen;
using RichardSzalay.MockHttp;
using System.Net.Http;
using Xunit;
using static Bunit.ComponentFactoryCollection;
namespace HotelManagementSystem.roostersTest
{
    [Obsolete]
    public class RosterPageTests : TestContext
    {
        [Fact]
        public void Should_Show_No_Schedule_Message_When_Empty()
        {
            Services.AddScoped<NotificationService>();
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://localhost:7016/api/schedule/list*")
                    .Respond("application/json", "[]");

            Services.AddHttpClient("test")
                .ConfigurePrimaryHttpMessageHandler(() => mockHttp);

            Services.AddSingleton<IHttpClientFactory>(sp =>
            {
                var factoryMock = new Mock<IHttpClientFactory>();
                factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                           .Returns(new HttpClient(mockHttp)
                           {
                               BaseAddress = new Uri("https://localhost:7016/")
                           });
                return factoryMock.Object;
            });

            // Act
            var cut = Render<Roster>();

            // Assert
            cut.Markup.Contains("No schedule found.");
        }
        [Fact]
        public void Should_Render_Table_When_Shifts_Exist()
        {
            var json = """
    [
        {
            "shiftID":1,
            "shiftDate":"2026-03-01T00:00:00",
            "staffName":"John Doe",
            "role":"Front Desk",
            "startTime":"08:00:00",
            "endTime":"16:00:00"
        }
    ]
    """;
            Services.AddScoped<NotificationService>();
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://localhost:7016/api/schedule/list*")
                    .Respond("application/json", json);

            Services.AddSingleton<IHttpClientFactory>(sp =>
            {
                var factoryMock = new Mock<IHttpClientFactory>();
                factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                           .Returns(new HttpClient(mockHttp)
                           {
                               BaseAddress = new Uri("https://localhost:7016/")
                           });
                return factoryMock.Object;
            });

            var cut = Render<Roster>();

            cut.Markup.Contains("John Doe");
            cut.Markup.Contains("Front Desk");
        }
        [Fact]
        public async Task Generate_Button_Should_Call_API()
        {
            Services.AddScoped<NotificationService>();
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://localhost:7016/api/schedule/generate")
                    .Respond(System.Net.HttpStatusCode.OK);

            Services.AddSingleton<IHttpClientFactory>(sp =>
            {
                var factoryMock = new Mock<IHttpClientFactory>();
                factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                           .Returns(new HttpClient(mockHttp)
                           {
                               BaseAddress = new Uri("https://localhost:7016/")
                           });
                return factoryMock.Object;
            });

            var cut = Render<Roster>();

            await cut.InvokeAsync(() =>
                cut.Find(".btn-generate").Click()
            );

            mockHttp.VerifyNoOutstandingExpectation();
        }
        [Fact]
        public void Should_Show_Error_When_API_Fails()
        {
            Services.AddScoped<NotificationService>();
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://localhost:7016/api/schedule/list*")
                    .Throw(new Exception("Connection failed"));

            Services.AddSingleton<IHttpClientFactory>(sp =>
            {
                var factoryMock = new Mock<IHttpClientFactory>();
                factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                           .Returns(new HttpClient(mockHttp)
                           {
                               BaseAddress = new Uri("https://localhost:7016/")
                           });
                return factoryMock.Object;
            });
            var cut = Render<Roster>();

            cut.Markup.Contains("Connection failed");
        }
    }
}