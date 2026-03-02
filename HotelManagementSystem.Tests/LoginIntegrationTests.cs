using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace HotelManagementSystem.loginIntergratrionTests
{
    public class LoginIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LoginIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Page_Should_Load()
        {
            var response = await _client.GetAsync("/login");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}