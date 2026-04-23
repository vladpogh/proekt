using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class AppointmentServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void BookAppointment_ShouldAddAppointment()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AppointmentService(db);
            var patientId = 1;
            var doctorId = 2;
            var date = DateTime.Now.AddDays(1);

            // Act
            var appt = service.BookAppointment(patientId, doctorId, date);

            // Assert
            Assert.NotNull(appt);
            Assert.Equal(1, db.Appointments.Count());
            Assert.Equal(AppointmentStatus.Pending, appt.Status);
        }

        [Fact]
        public void UpdateStatus_ShouldSetStatusToConfirmed()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AppointmentService(db);
            var appt = service.BookAppointment(1, 2, DateTime.Now.AddDays(1));

            // Act
            service.UpdateStatus(appt.Id, AppointmentStatus.Confirmed);

            // Assert
            var updated = db.Appointments.Find(appt.Id);
            Assert.Equal(AppointmentStatus.Confirmed, updated.Status);
        }

        [Fact]
        public void GetAvailableSlots_ShouldReturnCorrectSlots()
        {
            // Arrange
            var db = GetDbContext();
            var service = new AppointmentService(db);
            var profileService = new DoctorProfileService(db);
            var profile = new DoctorProfile 
            { 
                UserId = 1, 
                AvailableFromMinutes = 600, // 10:00
                AvailableToMinutes = 660,   // 11:00
                SlotDurationMinutes = 30,
                WorkingDays = "1,2,3,4,5"
            };
            db.DoctorProfiles.Add(profile);
            db.SaveChanges();

            var monday = new DateTime(2026, 4, 27); // A Monday

            // Act
            var slots = service.GetAvailableSlots(1, monday, profileService);

            // Assert
            // 10:00, 10:30 (11:00 is the end)
            Assert.Equal(2, slots.Count);
            Assert.Equal("10:00", slots[0].ToString(@"hh\:mm"));
            Assert.Equal("10:30", slots[1].ToString(@"hh\:mm"));
        }
    }
}
