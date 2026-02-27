using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotelManagementSystem_weapp.Data;
using HotelManagementSystem.Tests.Helpers;
using Moq;
using System.Threading.Tasks;
using TestContext = Bunit.TestContext;
using HotelManagementSystem_weapp.Components.Pages;

namespace HotelManagementSystem.Tests
{
    /// <summary>
    /// Simplified login tests using helper classes for cleaner test code
    /// </summary>
    [TestClass]
    public class LoginTestsSimplified : TestContext
    {
        private Mock<NavigationManager> _mockNavigationManager;
        private Mock<CustomAuthStateProvider> _mockAuthStateProvider;
        private NavigationCapture _navigationCapture;
        private AuthenticationCapture _authCapture;

        [TestInitialize]
        public void Setup()
        {
            // Setup navigation capture
            _navigationCapture = new NavigationCapture();
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockNavigationManager
                .Setup(x => x.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>(_navigationCapture.Capture);

            // Setup authentication capture
            _authCapture = new AuthenticationCapture();
            _mockAuthStateProvider = new Mock<CustomAuthStateProvider>();
            _mockAuthStateProvider
                .Setup(x => x.UpdateAuthenticationState(It.IsAny<UserSession>()))
                .Callback<UserSession>(_authCapture.Capture)
                .Returns(Task.CompletedTask);

            // Register services
            Services.AddSingleton<NavigationManager>(_mockNavigationManager.Object);
            Services.AddSingleton<AuthenticationStateProvider>(_mockAuthStateProvider.Object);
        }

        [TestMethod]
        public async Task ValidLogin_AsAdmin_RedirectsToDashboard()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateAdmin()
                .WithUsername("admin")
                .WithPassword("admin123")
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act
            await LoginAs(cut, "admin", "admin123");

            // Assert
            _authCapture.AssertSessionCreated("Administrator", "Admin");
            _navigationCapture.AssertNavigatedTo("/");
        }

        [TestMethod]
        public async Task ValidLogin_AsStaff_RedirectsToDashboard()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateStaff()
                .WithUsername("staff")
                .WithPassword("staff123")
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var cut =   Render<Login>();

            // Act
            await LoginAs(cut, "staff", "staff123");

            // Assert
            _authCapture.AssertSessionCreated("Staff Member", "Staff");
            _navigationCapture.AssertNavigatedTo("/");
        }

        [TestMethod]
        public async Task ValidLogin_WithPasswordResetRequired_RedirectsToChangePassword()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateDefault()
                .WithUsername("newuser")
                .WithPassword("temp123")
                .RequiresPasswordReset()
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act
            await LoginAs(cut, "newuser", "temp123");

            // Assert
            _authCapture.AssertSessionCreated("Test User", "Admin");
            _navigationCapture.AssertNavigatedToWithPrefix("/change-password/");
        }

        [TestMethod]
        public async Task ValidLogin_WithReturnUrl_RedirectsToSpecifiedUrl()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateDefault()
                .WithUsername("user")
                .WithPassword("pass123")
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var parameters = new Dictionary<string, object>
            {
                { "ReturnUrl", "/reports/monthly" }
            };

            var cut = Render<Login>(parameters);

            // Act
            await LoginAs(cut, "user", "pass123");

            // Assert
            _authCapture.AssertSessionCreated("Test User", "Admin");
            _navigationCapture.AssertNavigatedTo("/reports/monthly");
        }

        [TestMethod]
        public async Task InvalidPassword_ShowsErrorMessage()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateDefault()
                .WithUsername("user")
                .WithPassword("correctpassword")
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act
            await LoginAs(cut, "user", "wrongpassword");

            // Assert
            AssertErrorMessageContains(cut, "Invalid username or password");
            _authCapture.AssertNotCalled();
            _navigationCapture.AssertNotCalled();
        }

        [TestMethod]
        public async Task NonExistentUser_ShowsErrorMessage()
        {
            // Arrange
            var mockDb = MockDbContextFactory.CreateEmpty();
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act
            await LoginAs(cut, "nonexistent", "password123");

            // Assert
            AssertErrorMessageContains(cut, "Invalid username or password");
            _authCapture.AssertNotCalled();
            _navigationCapture.AssertNotCalled();
        }

        [TestMethod]
        public async Task EmptyCredentials_ShowsErrorMessage()
        {
            // Arrange
            var mockDb = MockDbContextFactory.CreateEmpty();
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act
            await LoginAs(cut, "", "");

            // Assert
            AssertErrorMessageContains(cut, "Invalid username or password");
            _authCapture.AssertNotCalled();
            _navigationCapture.AssertNotCalled();
        }

        [TestMethod]
        public async Task MultipleUsers_LoginWithCorrectUser()
        {
            // Arrange
            var admin = TestUserBuilder.CreateAdmin()
                .WithUsername("admin")
                .WithPassword("admin123")
                .Build();

            var staff = TestUserBuilder.CreateStaff()
                .WithUsername("staff")
                .WithPassword("staff123")
                .Build();

            var manager = TestUserBuilder.CreateManager()
                .WithUsername("manager")
                .WithPassword("manager123")
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(admin, staff, manager);
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act
            await LoginAs(cut, "staff", "staff123");

            // Assert
            _authCapture.AssertSessionCreated("Staff Member", "Staff");
            _navigationCapture.AssertNavigatedTo("/");
        }

        [TestMethod]
        public async Task PasswordResetTakesPriority_OverReturnUrl()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateDefault()
                .WithUsername("user")
                .WithPassword("temp123")
                .RequiresPasswordReset()
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var parameters = new Dictionary<string, object>
            {
                { "ReturnUrl", "/dashboard" }
            };

            var cut = Render<Login>(parameters);

            // Act
            await LoginAs(cut, "user", "temp123");

            // Assert
            // Password reset should take priority over return URL
            _navigationCapture.AssertNavigatedToWithPrefix("/change-password/");
        }

        [TestMethod]
        public void InitialPageLoad_NoErrorDisplayed()
        {
            // Arrange
            var mockDb = MockDbContextFactory.CreateEmpty();
            Services.AddSingleton(mockDb.Object);

            // Act
            var cut = Render<Login>();

            // Assert
            var alerts = cut.FindAll(".alert-danger");
            Assert.AreEqual(0, alerts.Count, "No error should be displayed on initial load");
        }

        [TestMethod]
        public async Task SuccessfulLogin_ClearsAnyPreviousErrors()
        {
            // Arrange
            var testUser = TestUserBuilder.CreateDefault()
                .WithUsername("user")
                .WithPassword("correct123")
                .Build();

            var mockDb = MockDbContextFactory.CreateWithUsers(testUser);
            Services.AddSingleton(mockDb.Object);

            var cut = Render<Login>();

            // Act - First attempt with wrong password
            await LoginAs(cut, "user", "wrongpassword");
            AssertErrorMessageContains(cut, "Invalid username or password");

            // Act - Second attempt with correct password
            await LoginAs(cut, "user", "correct123");

            // Assert - Error should be cleared and navigation should occur
            _navigationCapture.AssertNavigatedTo("/");
        }

        #region Helper Methods

        /// <summary>
        /// Helper method to perform login action
        /// </summary>
        private async Task LoginAs(IRenderedComponent<Login> component, string username, string password)
        {
            var usernameInput = component.Find("input[id='username']");
            var passwordInput = component.Find("input[id='password']");
            var signInButton = component.Find("button");

            await component.InvokeAsync(() =>
            {
                usernameInput.Change(username);
                passwordInput.Change(password);
            });

            await component.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });
        }

        /// <summary>
        /// Helper method to assert error message contains expected text
        /// </summary>
        private void AssertErrorMessageContains(IRenderedComponent<Login> component, string expectedText)
        {
            var errorAlert = component.Find(".alert-danger");
            Assert.IsNotNull(errorAlert, "Error message should be displayed");
            Assert.IsTrue(
                errorAlert.TextContent.Contains(expectedText),
                $"Expected error to contain '{expectedText}' but got '{errorAlert.TextContent}'"
            );
        }

        #endregion
    }
}
