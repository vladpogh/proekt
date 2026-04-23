using proekt.Data;
using proekt.Models;
using Microsoft.EntityFrameworkCore;

namespace proekt.Services;

public class AppointmentService
{
    private readonly ApplicationDbContext _db;

    public AppointmentService(ApplicationDbContext db) => _db = db;

    // ── Booking ────────────────────────────────────────────────────────────────

    public Appointment BookAppointment(int patientId, int doctorId, DateTime dateTime, string? notes = null)
    {
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = dateTime,
            Status = AppointmentStatus.Pending,
            PatientNotes = notes,
            CreatedAt = DateTime.Now
        };
        _db.Appointments.Add(appointment);
        _db.SaveChanges();
        return appointment;
    }

    public Appointment? GetById(int id)
        => _db.Appointments.FirstOrDefault(a => a.Id == id);

    // ── Queries ────────────────────────────────────────────────────────────────

    public List<Appointment> GetByPatient(int patientId)
        => _db.Appointments
              .Where(a => a.PatientId == patientId)
              .OrderByDescending(a => a.AppointmentDate)
              .ToList();

    public List<Appointment> GetByDoctor(int doctorId)
        => _db.Appointments
              .Where(a => a.DoctorId == doctorId)
              .OrderByDescending(a => a.AppointmentDate)
              .ToList();

    public List<Appointment> GetAll()
        => _db.Appointments
              .OrderByDescending(a => a.AppointmentDate)
              .ToList();

    public List<Appointment> GetUpcomingByDoctor(int doctorId)
        => _db.Appointments
              .Where(a => a.DoctorId == doctorId
                       && a.AppointmentDate >= DateTime.Now
                       && a.Status != AppointmentStatus.Cancelled)
              .OrderBy(a => a.AppointmentDate)
              .ToList();

    // ── Status updates ─────────────────────────────────────────────────────────

    public bool UpdateStatus(int id, AppointmentStatus status, string? doctorNotes = null)
    {
        var appt = _db.Appointments.Find(id);
        if (appt == null) return false;
        appt.Status = status;
        if (doctorNotes != null) appt.DoctorNotes = doctorNotes;
        _db.SaveChanges();
        return true;
    }

    public bool Cancel(int id)
        => UpdateStatus(id, AppointmentStatus.Cancelled);

    // ── Available slots ────────────────────────────────────────────────────────

    /// <summary>Returns DateTime slots for a given doctor on a given date,
    /// excluding already-booked (non-cancelled) slots.</summary>
    public List<DateTime> GetAvailableSlots(int doctorId, DateTime date,
        DoctorProfileService profileService)
    {
        var profile = profileService.GetByUserId(doctorId);
        if (profile == null) return new List<DateTime>();

        var from = date.Date.AddMinutes(profile.AvailableFromMinutes);
        var to   = date.Date.AddMinutes(profile.AvailableToMinutes);
        var slot = profile.SlotDurationMinutes;

        var booked = _db.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate.Date == date.Date
                     && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentDate)
            .ToHashSet();

        var slots = new List<DateTime>();
        for (var t = from; t < to; t = t.AddMinutes(slot))
        {
            if (!booked.Contains(t))
                slots.Add(t);
        }
        return slots;
    }

    // ── Stats ──────────────────────────────────────────────────────────────────

    public int CountCompletedByDoctor(int doctorId, DateTime from, DateTime to)
        => _db.Appointments.Count(a =>
            a.DoctorId == doctorId &&
            a.Status == AppointmentStatus.Completed &&
            a.AppointmentDate >= from &&
            a.AppointmentDate <= to);

    public int GetTotalCount() => _db.Appointments.Count();
}
