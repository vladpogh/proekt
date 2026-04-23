using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class StatisticsServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void GetPlatformSummary_ShouldCalculateCorrectTotals()
        {
            // Arrange
            var db = GetDbContext();
            var service = new StatisticsService(db);
            
            db.Users.Add(new User { Role = UserRole.Doctor });
            db.Users.Add(new User { Role = UserRole.User });
            db.Appointments.Add(new Appointment());
            db.Payments.Add(new Payment { Status = PaymentStatus.Paid, Amount = 100 });
            db.Payments.Add(new Payment { Status = PaymentStatus.Paid, Amount = 50 });
            db.Payments.Add(new Payment { Status = PaymentStatus.Pending, Amount = 200 });
            db.SaveChanges();

            // Act
            var summary = service.GetPlatformSummary();

            // Assert
            Assert.Equal(2, summary.TotalUsers);
            Assert.Equal(1, summary.TotalDoctors);
            Assert.Equal(1, summary.TotalAppointments);
            Assert.Equal(150m, summary.TotalRevenue);
        }

        [Fact]
        public void GetMostCommonDiagnoses_ShouldReturnOrderedList()
        {
            // Arrange
            var db = GetDbContext();
            var service = new StatisticsService(db);
            db.MedicalEntries.Add(new MedicalEntry { Title = "Flu", Type = MedicalEntryType.Diagnosis });
            db.MedicalEntries.Add(new MedicalEntry { Title = "Flu", Type = MedicalEntryType.Diagnosis });
            db.MedicalEntries.Add(new MedicalEntry { Title = "Cold", Type = MedicalEntryType.Diagnosis });
            db.SaveChanges();

            // Act
            var diag = service.GetMostCommonDiagnoses(10);

            // Assert
            Assert.Equal(2, diag.Count);
            Assert.Equal("Flu", diag[0].Diagnosis);
            Assert.Equal(2, diag[0].Count);
            Assert.Equal("Cold", diag[1].Diagnosis);
            Assert.Equal(1, diag[1].Count);
        }
    }
}
