using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;

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
        public void RegisterUser_ShouldStoreHashedPassword_NotPlainText()
        {
            // Arrange
            var db = GetDbContext();
            var service = new UserService(db);
            var password = "securePassword!";

            // Act
            service.RegisterUser("Hash Test", "hash@test.com", password);

            // Assert: the stored password must NEVER equal the plain-text value
            var stored = db.Users.First().Password;
            Assert.NotEqual(password, stored);
            // BCrypt hashes always start with the $2a$ or $2b$ identifier
            Assert.StartsWith("$2", stored);
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
            service.RegisterUser("role@test.com", "role@test.com", "pass");
            var user = db.Users.First();

            // Act
            service.UpdateUserRole(user.Id, UserRole.Doctor);

            // Assert
            var updatedUser = db.Users.Find(user.Id);
            Assert.Equal(UserRole.Doctor, updatedUser.Role);
        }

        [Fact]
        public void ChangePassword_ShouldUpdateHashAndVerifyCorrectly()
        {
            // Arrange
            var db = GetDbContext();
            var service = new UserService(db);
            service.RegisterUser("Change Pass", "change@test.com", "oldPass");
            var user = db.Users.First();

            // Act
            service.ChangePassword(user.Id, "newStrongPass!");

            // Assert: old password no longer works
            Assert.False(service.VerifyPassword(user.Id, "oldPass"));
            // Assert: new password works
            Assert.True(service.VerifyPassword(user.Id, "newStrongPass!"));
            // Assert: new password is stored as a BCrypt hash
            Assert.StartsWith("$2", db.Users.Find(user.Id)!.Password);
        }
    }
}

