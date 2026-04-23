using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class PrescriptionServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CreatePrescription_ShouldAddPrescription()
        {
            // Arrange
            var db = GetDbContext();
            var service = new PrescriptionService(db);
            var doctor = new User { Id = 1, FullName = "Dr. Smith" };
            var patient = new User { Id = 2, FullName = "John Doe" };
            db.Users.AddRange(doctor, patient);
            db.SaveChanges();

            // Act
            var rx = service.CreatePrescription(2, 1, "Dr. Smith", "Flu", "Paracetamol", "Rest and fluids");

            // Assert
            Assert.NotNull(rx);
            Assert.Equal(1, db.Prescriptions.Count());
            Assert.Equal("Dr. Smith", rx.DoctorName);
        }

        [Fact]
        public void GetByPatient_ShouldReturnUserPrescriptions()
        {
            // Arrange
            var db = GetDbContext();
            var service = new PrescriptionService(db);
            db.Users.Add(new User { Id = 1, FullName = "Patient" });
            db.Prescriptions.Add(new Prescription { PatientId = 1, Diagnosis = "D1" });
            db.Prescriptions.Add(new Prescription { PatientId = 2, Diagnosis = "D2" });
            db.SaveChanges();

            // Act
            var list = service.GetByPatient(1);

            // Assert
            Assert.Single(list);
            Assert.Equal("D1", list.First().Diagnosis);
        }
    }
}
