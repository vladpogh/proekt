using proekt.Data;
using proekt.Models;

namespace proekt.Services;

public class PrescriptionService
{
    private readonly ApplicationDbContext _db;

    public PrescriptionService(ApplicationDbContext db) => _db = db;

    public Prescription CreatePrescription(int patientId, int doctorId, string doctorName,
        string diagnosis, string medications, string? instructions, DateTime? expiresAt = null)
    {
        var prescription = new Prescription
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DoctorName = doctorName,
            Diagnosis = diagnosis,
            Medications = medications,
            Instructions = instructions,
            IssuedAt = DateTime.Now,
            ExpiresAt = expiresAt,
            IsActive = true
        };
        _db.Prescriptions.Add(prescription);
        _db.SaveChanges();
        return prescription;
    }

    public Prescription? GetById(int id)
        => _db.Prescriptions.FirstOrDefault(p => p.Id == id);

    public List<Prescription> GetByPatient(int patientId)
        => _db.Prescriptions
              .Where(p => p.PatientId == patientId)
              .OrderByDescending(p => p.IssuedAt)
              .ToList();

    public List<Prescription> GetByDoctor(int doctorId)
        => _db.Prescriptions
              .Where(p => p.DoctorId == doctorId)
              .OrderByDescending(p => p.IssuedAt)
              .ToList();

    public List<Prescription> GetAll()
        => _db.Prescriptions.OrderByDescending(p => p.IssuedAt).ToList();

    public bool Deactivate(int id)
    {
        var p = _db.Prescriptions.Find(id);
        if (p == null) return false;
        p.IsActive = false;
        _db.SaveChanges();
        return true;
    }

    public int GetTotalCount() => _db.Prescriptions.Count();
}
