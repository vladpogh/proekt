using proekt.Data;
using proekt.Models;

namespace proekt.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _db;

    public PaymentService(ApplicationDbContext db) => _db = db;

    public Payment InitiatePayment(int appointmentId, int patientId, int doctorId, decimal amount, string currency = "EUR")
    {
        // Avoid duplicate pending payment for the same appointment
        var existing = _db.Payments.FirstOrDefault(p =>
            p.AppointmentId == appointmentId && p.Status == PaymentStatus.Pending);
        if (existing != null) return existing;

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            DoctorId = doctorId,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.Now
        };
        _db.Payments.Add(payment);
        _db.SaveChanges();
        return payment;
    }

    public bool ConfirmPayment(int paymentId)
    {
        var payment = _db.Payments.Find(paymentId);
        if (payment == null || payment.Status != PaymentStatus.Pending) return false;
        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.Now;
        _db.SaveChanges();
        return true;
    }

    public bool RefundPayment(int paymentId)
    {
        var payment = _db.Payments.Find(paymentId);
        if (payment == null || payment.Status != PaymentStatus.Paid) return false;
        payment.Status = PaymentStatus.Refunded;
        _db.SaveChanges();
        return true;
    }

    public Payment? GetById(int id)
        => _db.Payments.FirstOrDefault(p => p.Id == id);

    public Payment? GetByAppointment(int appointmentId)
        => _db.Payments.FirstOrDefault(p => p.AppointmentId == appointmentId);

    public List<Payment> GetByPatient(int patientId)
        => _db.Payments
              .Where(p => p.PatientId == patientId)
              .OrderByDescending(p => p.CreatedAt)
              .ToList();

    public List<Payment> GetByDoctor(int doctorId)
        => _db.Payments
              .Where(p => p.DoctorId == doctorId)
              .OrderByDescending(p => p.CreatedAt)
              .ToList();

    public List<Payment> GetAll()
        => _db.Payments.OrderByDescending(p => p.CreatedAt).ToList();

    public decimal GetTotalRevenue()
        => _db.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
}
