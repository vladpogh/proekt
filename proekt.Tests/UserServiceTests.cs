using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System.Collections.Generic;
using System.Linq;

namespace proekt.Tests
{
    public class UserServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void RegisterUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var db = GetDbContext();
            var service = new UserService(db);
            var email = "test@example.com";
            var password = "password123";
            var fullName = "Test User";

            // Act
            var result = service.RegisterUser(fullName, email, password);

            // Assert
            Assert.True(result);
            Assert.Equal(1, db.Users.Count());
            Assert.Equal(email, db.Users.First().Email);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenCredentialsAreValid()
        {
            // Arrange
            var db = GetDbContext();
            var service = new UserService(db);
            var email = "auth@test.com";
            var password = "secretPassword";
            service.RegisterUser("Auth User", email, password);

            // Act
            var isValid = service.VerifyPassword(email, password);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            var db = GetDbContext();
            var service = new UserService(db);
            var email = "wrong@test.com";
            service.RegisterUser("User", email, "correct");

            // Act
            var isValid = service.VerifyPassword(email, "wrong");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void UpdateUserRole_ShouldChangeRole()
        {
            // Arrange
            var db = GetDbContext();
            var service = new UserService(db);
            service.RegisterUser("role@test.com", "pass", "User");
            var user = db.Users.First();

            // Act
            service.UpdateUserRole(user.Id, UserRole.Doctor);

            // Assert
            var updatedUser = db.Users.Find(user.Id);
            Assert.Equal(UserRole.Doctor, updatedUser.Role);
        }
    }
}
