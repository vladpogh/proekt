using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class ActivityLogServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Log_ShouldAddLogToDatabase()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ActivityLogService(db);

            // Act
            service.Log(1, "Test User", "Login", "Successful");

            // Assert
            Assert.Equal(1, db.ActivityLogs.Count());
            Assert.Equal("Login", db.ActivityLogs.First().Action);
            Assert.Equal("Successful", db.ActivityLogs.First().Details);
        }

        [Fact]
        public void GetRecent_ShouldReturnLogsOrderedByTime()
        {
            // Arrange
            var db = GetDbContext();
            var service = new ActivityLogService(db);
            service.Log(1, "U1", "A1", "D1");
            service.Log(2, "U2", "A2", "D2");

            // Act
            var logs = service.GetRecent(10);

            // Assert
            Assert.Equal(2, logs.Count);
            Assert.Equal("A2", logs[0].Action); // Recent first
        }
    }
}
