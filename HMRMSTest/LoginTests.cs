using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotelManagementSystem_weapp.Data;
using Moq;
using System;
using System.Threading.Tasks;
using TestContext = Bunit.TestContext;
using HotelManagementSystem_weapp.Models;
using HotelManagementSystem_weapp.Components.Pages;

namespace HotelManagementSystem.Tests
{
    [TestClass]
    public class LoginTests : TestContext
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<NavigationManager> _mockNavigationManager;
        private Mock<CustomAuthStateProvider> _mockAuthStateProvider;
        private Mock<DbSet<ApplicationUser>> _mockUserDbSet;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockAuthStateProvider = new Mock<CustomAuthStateProvider>();
            _mockUserDbSet = new Mock<DbSet<ApplicationUser>>();

            // Register services
            Services.AddSingleton(_mockDbContext.Object);
            Services.AddSingleton<NavigationManager>(_mockNavigationManager.Object);
            Services.AddSingleton<AuthenticationStateProvider>(_mockAuthStateProvider.Object);
        }

        [TestMethod]
        public void LoginPage_RendersCorrectly()
        {
            // Arrange & Act
            var cut = Render<Login>();

            // Assert
            Assert.IsNotNull(cut.Find("input[id='username']"), "Username input should be rendered");
            Assert.IsNotNull(cut.Find("input[id='password']"), "Password input should be rendered");
            Assert.IsNotNull(cut.Find("button"), "Sign In button should be rendered");
            
            // Check for proper labels
            var usernameLabel = cut.Find("label[for='username']");
            Assert.AreEqual("Username", usernameLabel.TextContent);
            
            var passwordLabel = cut.Find("label[for='password']");
            Assert.AreEqual("Password", passwordLabel.TextContent);
        }

        [TestMethod]
        public void LoginPage_DisplaysBrandingElements()
        {
            // Arrange & Act
            var cut =   Render<Login>();

            // Assert
            Assert.IsTrue(cut.Markup.Contains("Hotel Pro"), "Should display Hotel Pro branding");
            Assert.IsTrue(cut.Markup.Contains("Welcome back"), "Should display welcome message");
        }

        [TestMethod]
        public async Task HandleLogin_WithValidCredentials_NavigatesToDashboard()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FullName = "Test User",
                Role = "Admin",
                IsPasswordResetRequired = false
            };

            var users = new List<ApplicationUser> { testUser }.AsQueryable();
            
            var mockSet = CreateMockDbSet(users);
            _mockDbContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);

            var sessionUpdated = false;
            _mockAuthStateProvider
                .Setup(x => x.UpdateAuthenticationState(It.IsAny<UserSession>()))
                .Callback<UserSession>(session =>
                {
                    Assert.AreEqual("Test User", session.UserName);
                    Assert.AreEqual("Admin", session.Role);
                    sessionUpdated = true;
                })
                .Returns(Task.CompletedTask);

            var navigationCalled = false;
            _mockNavigationManager
                .Setup(x => x.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((url, forceLoad) =>
                {
                    Assert.AreEqual("/", url);
                    navigationCalled = true;
                });

            var cut = Render<Login>();

            // Act
            var usernameInput = cut.Find("input[id='username']");
            var passwordInput = cut.Find("input[id='password']");
            var signInButton = cut.Find("button");

            await cut.InvokeAsync(() =>
            {
                usernameInput.Change("testuser");
                passwordInput.Change("password123");
            });

            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            Assert.IsTrue(sessionUpdated, "Authentication state should be updated");
            Assert.IsTrue(navigationCalled, "Navigation should be called");
        }

        [TestMethod]
        public async Task HandleLogin_WithInvalidPassword_ShowsErrorMessage()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                FullName = "Test User",
                Role = "Admin"
            };

            var users = new List<ApplicationUser> { testUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _mockDbContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);

            var cut = Render<Login>();

            // Act
            var usernameInput = cut.Find("input[id='username']");
            var passwordInput = cut.Find("input[id='password']");
            var signInButton = cut.Find("button");

            await cut.InvokeAsync(() =>
            {
                usernameInput.Change("testuser");
                passwordInput.Change("wrongpassword");
            });

            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            var errorAlert = cut.Find(".alert-danger");
            Assert.IsNotNull(errorAlert, "Error message should be displayed");
            Assert.IsTrue(errorAlert.TextContent.Contains("Invalid username or password"));
        }

        [TestMethod]
        public async Task HandleLogin_WithNonExistentUser_ShowsErrorMessage()
        {
            // Arrange
            var users = new List<ApplicationUser>().AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _mockDbContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);

            var cut = Render<Login>();

            // Act
            var usernameInput = cut.Find("input[id='username']");
            var passwordInput = cut.Find("input[id='password']");
            var signInButton = cut.Find("button");

            await cut.InvokeAsync(() =>
            {
                usernameInput.Change("nonexistent");
                passwordInput.Change("password123");
            });

            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            var errorAlert = cut.Find(".alert-danger");
            Assert.IsNotNull(errorAlert, "Error message should be displayed");
            Assert.IsTrue(errorAlert.TextContent.Contains("Invalid username or password"));
        }

        [TestMethod]
        public async Task HandleLogin_WithPasswordResetRequired_NavigatesToChangePassword()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FullName = "Test User",
                Role = "Admin",
                IsPasswordResetRequired = true
            };

            var users = new List<ApplicationUser> { testUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _mockDbContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);

            _mockAuthStateProvider
                .Setup(x => x.UpdateAuthenticationState(It.IsAny<UserSession>()))
                .Returns(Task.CompletedTask);

            string? navigatedUrl = null;
            _mockNavigationManager
                .Setup(x => x.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((url, forceLoad) => { navigatedUrl = url; });

            var cut = Render<Login>();

            // Act
            var usernameInput = cut.Find("input[id='username']");
            var passwordInput = cut.Find("input[id='password']");
            var signInButton = cut.Find("button");

            await cut.InvokeAsync(() =>
            {
                usernameInput.Change("testuser");
                passwordInput.Change("password123");
            });

            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            Assert.IsNotNull(navigatedUrl);
            Assert.IsTrue(navigatedUrl.StartsWith("/change-password/"));
        }

        [TestMethod]
        public async Task HandleLogin_WithReturnUrl_NavigatesToReturnUrl()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FullName = "Test User",
                Role = "Admin",
                IsPasswordResetRequired = false
            };

            var users = new List<ApplicationUser> { testUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _mockDbContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);

            _mockAuthStateProvider
                .Setup(x => x.UpdateAuthenticationState(It.IsAny<UserSession>()))
                .Returns(Task.CompletedTask);

            string? navigatedUrl = null;
            _mockNavigationManager
                .Setup(x => x.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, bool>((url, forceLoad) => { navigatedUrl = url; });

            var parameters = new Dictionary<string, object>
            {
                { "ReturnUrl", "/dashboard/reports" }
            };

            var cut = Render<Login>(parameters);

            // Act
            var usernameInput = cut.Find("input[id='username']");
            var passwordInput = cut.Find("input[id='password']");
            var signInButton = cut.Find("button");

            await cut.InvokeAsync(() =>
            {
                usernameInput.Change("testuser");
                passwordInput.Change("password123");
            });

            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            Assert.AreEqual("/dashboard/reports", navigatedUrl);
        }

        [TestMethod]
        public void LoginPage_InitialState_NoErrorMessage()
        {
            // Arrange & Act
            var cut = Render<Login>();

            // Assert
            var alerts = cut.FindAll(".alert-danger");
            Assert.AreEqual(0, alerts.Count, "No error message should be displayed initially");
        }

        [TestMethod]
        public async Task HandleLogin_EmptyCredentials_ShowsErrorMessage()
        {
            // Arrange
            var users = new List<ApplicationUser>().AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _mockDbContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);

            var cut = Render<Login>();

            // Act
            var signInButton = cut.Find("button");
            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            var errorAlert = cut.Find(".alert-danger");
            Assert.IsNotNull(errorAlert, "Error message should be displayed for empty credentials");
        }

        [TestMethod]
        public async Task HandleLogin_DatabaseException_ShowsGenericErrorMessage()
        {
            // Arrange
            _mockDbContext.Setup(db => db.ApplicationUsers)
                .Throws(new Exception("Database connection failed"));

            var cut = Render<Login>();

            // Act
            var usernameInput = cut.Find("input[id='username']");
            var passwordInput = cut.Find("input[id='password']");
            var signInButton = cut.Find("button");

            await cut.InvokeAsync(() =>
            {
                usernameInput.Change("testuser");
                passwordInput.Change("password123");
            });

            await cut.InvokeAsync(async () =>
            {
                await signInButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            });

            // Assert
            var errorAlert = cut.Find(".alert-danger");
            Assert.IsNotNull(errorAlert);
            Assert.IsTrue(errorAlert.TextContent.Contains("A database error occurred"));
        }

        // Helper method to create a mock DbSet
        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(data.Provider));

            return mockSet;
        }
    }

    // Helper classes for async query support
    internal class TestAsyncQueryProvider<TEntity> : IQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}
