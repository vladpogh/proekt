using Xunit;
using Microsoft.EntityFrameworkCore;
using proekt.Data;
using proekt.Models;
using proekt.Services;
using System;
using System.Linq;

namespace proekt.Tests
{
    public class PaymentServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void InitiatePayment_ShouldCreatePendingPayment()
        {
            // Arrange
            var db = GetDbContext();
            var service = new PaymentService(db);

            // Act
            var payment = service.InitiatePayment(1, 1, 2, 100, "BGN");

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(PaymentStatus.Pending, payment.Status);
            Assert.Equal(100, payment.Amount);
        }

        [Fact]
        public void ConfirmPayment_ShouldSetStatusToPaid()
        {
            // Arrange
            var db = GetDbContext();
            var service = new PaymentService(db);
            var payment = service.InitiatePayment(1, 1, 2, 50, "BGN");

            // Act
            var result = service.ConfirmPayment(payment.Id);

            // Assert
            Assert.True(result);
            var updated = db.Payments.Find(payment.Id);
            Assert.NotNull(updated);
            Assert.Equal(PaymentStatus.Paid, updated.Status);
            Assert.NotNull(updated.PaidAt);
        }
    }
}
