using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class DoctorProfileServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Update_ShouldCreateIfNotExist()
        {
            // Arrange
            var db = GetDbContext();
            var service = new DoctorProfileService(db);
            var userId = 1;

            // Act
            service.Update(userId, "Spec", "Bio", 100, 480, 1020, "1,2,3", 30);

            // Assert
            var profile = db.DoctorProfiles.FirstOrDefault(p => p.UserId == userId);
            Assert.NotNull(profile);
            Assert.Equal("Spec", profile.Specialization);
        }

        [Fact]
        public void Update_ShouldUpdateIfExist()
        {
            // Arrange
            var db = GetDbContext();
            var service = new DoctorProfileService(db);
            db.DoctorProfiles.Add(new DoctorProfile { UserId = 1, Specialization = "Old" });
            db.SaveChanges();

            // Act
            service.Update(1, "New", "Bio", 50, 0, 0, "1", 15);

            // Assert
            var profile = db.DoctorProfiles.First(p => p.UserId == 1);
            Assert.Equal("New", profile.Specialization);
        }
    }
}
