using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class MedicalRecordServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CreateEmptyRecord_ShouldAddRecord()
        {
            // Arrange
            var db = GetDbContext();
            var service = new MedicalRecordService(db);
            var userId = 1;

            // Act
            var record = service.CreateEmptyRecord(userId);

            // Assert
            Assert.NotNull(record);
            Assert.Equal(userId, record.UserId);
            Assert.Equal(1, db.MedicalRecords.Count());
        }

        [Fact]
        public void AddEntry_ShouldAddEntryToRecord()
        {
            // Arrange
            var db = GetDbContext();
            var service = new MedicalRecordService(db);
            var userId = 1;

            // Act
            service.AddEntry(userId, MedicalEntryType.Diagnosis, "Title", "Content", DateTime.Now, 2, "Dr. Test");

            // Assert
            Assert.Equal(1, db.MedicalEntries.Count());
            Assert.Equal("Title", db.MedicalEntries.First().Title);
        }
    }
}
