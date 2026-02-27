using HotelManagementSystem_weapp.Data;
using HotelManagementSystem_weapp.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace HotelManagementSystem.Tests.Helpers
{
    /// <summary>
    /// Helper class to build test ApplicationUser objects with fluent API
    /// </summary>
    public class TestUserBuilder
    {
        private int _id = 1;
        private string _username = "testuser";
        private string _password = "password123";
        private string _fullName = "Test User";
        private string _role = "Admin";
        private bool _isPasswordResetRequired = false;
        private string _email = "test@example.com";

        public TestUserBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public TestUserBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public TestUserBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public TestUserBuilder WithFullName(string fullName)
        {
            _fullName = fullName;
            return this;
        }

        public TestUserBuilder WithRole(string role)
        {
            _role = role;
            return this;
        }

        public TestUserBuilder RequiresPasswordReset(bool required = true)
        {
            _isPasswordResetRequired = required;
            return this;
        }

        public TestUserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public ApplicationUser Build()
        {
            return new ApplicationUser
            {
                Id = _id,
                Username = _username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(_password),
                FullName = _fullName,
                Role = _role,
                IsPasswordResetRequired = _isPasswordResetRequired
               
            };
        }

        public static TestUserBuilder CreateDefault()
        {
            return new TestUserBuilder();
        }

        public static TestUserBuilder CreateAdmin()
        {
            return new TestUserBuilder()
                .WithRole("Admin")
                .WithUsername("admin")
                .WithFullName("Administrator");
        }

        public static TestUserBuilder CreateStaff()
        {
            return new TestUserBuilder()
                .WithRole("Staff")
                .WithUsername("staff")
                .WithFullName("Staff Member");
        }

        public static TestUserBuilder CreateManager()
        {
            return new TestUserBuilder()
                .WithRole("Manager")
                .WithUsername("manager")
                .WithFullName("Manager");
        }
    }

    /// <summary>
    /// Helper class to create mock DbContext with test data
    /// </summary>
    public static class MockDbContextFactory
    {
        public static Mock<ApplicationDbContext> CreateWithUsers(params ApplicationUser[] users)
        {
            var mockContext = new Mock<ApplicationDbContext>();
            var userList = users.ToList();
            var mockSet = CreateMockDbSet(userList.AsQueryable());
            
            mockContext.Setup(db => db.ApplicationUsers).Returns(mockSet.Object);
            
            return mockContext;
        }

        public static Mock<ApplicationDbContext> CreateEmpty()
        {
            return CreateWithUsers();
        }

        public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
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

    /// <summary>
    /// Helper class for navigation assertions
    /// </summary>
    public class NavigationCapture
    {
        public string? Url { get; private set; }
        public bool ForceLoad { get; private set; }
        public bool WasCalled { get; private set; }

        public void Capture(string url, bool forceLoad)
        {
            Url = url;
            ForceLoad = forceLoad;
            WasCalled = true;
        }

        public void AssertNavigatedTo(string expectedUrl)
        {
            if (!WasCalled)
                throw new AssertFailedException("Navigation was not called");
            
            if (Url != expectedUrl)
                throw new AssertFailedException($"Expected navigation to '{expectedUrl}' but got '{Url}'");
        }

        public void AssertNavigatedToWithPrefix(string expectedPrefix)
        {
            if (!WasCalled)
                throw new AssertFailedException("Navigation was not called");
            
            if (Url == null || !Url.StartsWith(expectedPrefix))
                throw new AssertFailedException($"Expected navigation URL to start with '{expectedPrefix}' but got '{Url}'");
        }

        public void AssertNotCalled()
        {
            if (WasCalled)
                throw new AssertFailedException($"Expected no navigation but navigated to '{Url}'");
        }
    }

    /// <summary>
    /// Helper class for authentication state assertions
    /// </summary>
    public class AuthenticationCapture
    {
        public UserSession? CapturedSession { get; private set; }
        public bool WasCalled { get; private set; }

        public void Capture(UserSession session)
        {
            CapturedSession = session;
            WasCalled = true;
        }

        public void AssertSessionCreated(string expectedUserName, string expectedRole)
        {
            if (!WasCalled)
                throw new AssertFailedException("Authentication state was not updated");
            
            if (CapturedSession == null)
                throw new AssertFailedException("Session was null");
            
            if (CapturedSession.UserName != expectedUserName)
                throw new AssertFailedException($"Expected username '{expectedUserName}' but got '{CapturedSession.UserName}'");
            
            if (CapturedSession.Role != expectedRole)
                throw new AssertFailedException($"Expected role '{expectedRole}' but got '{CapturedSession.Role}'");
        }

        public void AssertNotCalled()
        {
            if (WasCalled)
                throw new AssertFailedException("Expected authentication state not to be updated");
        }
    }

    /// <summary>
    /// Exception for custom assertions
    /// </summary>
    public class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message)
        {
        }
    }

    // Async query support classes
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
