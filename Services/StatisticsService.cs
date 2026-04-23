using proekt.Data;
using proekt.Models;
using Microsoft.EntityFrameworkCore;

namespace proekt.Services;

public class StatisticsService
{
    private readonly ApplicationDbContext _db;

    public StatisticsService(ApplicationDbContext db) => _db = db;

    /// <summary>Top N most frequent diagnosis titles in MedicalEntries.</summary>
    public List<(string Diagnosis, int Count)> GetMostCommonDiagnoses(int top = 10)
        => _db.MedicalEntries
              .Where(e => e.Type == MedicalEntryType.Diagnosis && !e.IsDeleted)
              .GroupBy(e => e.Title)
              .Select(g => new { Title = g.Key, Count = g.Count() })
              .OrderByDescending(g => g.Count)
              .Take(top)
              .AsEnumerable()
              .Select(g => (g.Title, g.Count))
              .ToList();

    /// <summary>Appointment count per day for the last N days.</summary>
    public List<(DateTime Date, int Count)> GetVisitFrequency(int days = 30)
    {
        var from = DateTime.Today.AddDays(-days);
        return _db.Appointments
                  .Where(a => a.AppointmentDate >= from)
                  .GroupBy(a => a.AppointmentDate.Date)
                  .Select(g => new { Date = g.Key, Count = g.Count() })
                  .OrderBy(g => g.Date)
                  .AsEnumerable()
                  .Select(g => (g.Date, g.Count))
                  .ToList();
    }

    /// <summary>Platform summary counters.</summary>
    public PlatformSummary GetPlatformSummary()
        => new PlatformSummary
        {
            TotalUsers        = _db.Users.Count(),
            TotalDoctors      = _db.Users.Count(u => u.Role == UserRole.Doctor),
            TotalRecords      = _db.MedicalRecords.Count(),
            TotalAppointments = _db.Appointments.Count(),
            TotalPrescriptions = _db.Prescriptions.Count(),
            TotalRevenue      = _db.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0m
        };
}

public class PlatformSummary
{
    public int TotalUsers { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalRecords { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalPrescriptions { get; set; }
    public decimal TotalRevenue { get; set; }
}
