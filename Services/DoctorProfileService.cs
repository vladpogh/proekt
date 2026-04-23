using proekt.Data;
using proekt.Models;

namespace proekt.Services;

public class DoctorProfileService
{
    private readonly ApplicationDbContext _db;

    public DoctorProfileService(ApplicationDbContext db) => _db = db;

    // ── Get or create ──────────────────────────────────────────────────────────

    public DoctorProfile GetOrCreate(int userId)
    {
        var profile = _db.DoctorProfiles.FirstOrDefault(d => d.UserId == userId);
        if (profile != null) return profile;

        profile = new DoctorProfile { UserId = userId };
        _db.DoctorProfiles.Add(profile);
        _db.SaveChanges();
        return profile;
    }

    public DoctorProfile? GetByUserId(int userId)
        => _db.DoctorProfiles.FirstOrDefault(d => d.UserId == userId);

    public DoctorProfile? GetById(int id)
        => _db.DoctorProfiles.FirstOrDefault(d => d.Id == id);

    // ── Update ─────────────────────────────────────────────────────────────────

    public void Update(int userId, string specialization, string? bio,
        decimal consultationFee, int fromMinutes, int toMinutes,
        string workingDays, int slotDurationMinutes)
    {
        var profile = GetOrCreate(userId);
        profile.Specialization = specialization;
        profile.Bio = bio;
        profile.ConsultationFee = consultationFee;
        profile.AvailableFromMinutes = fromMinutes;
        profile.AvailableToMinutes = toMinutes;
        profile.WorkingDays = workingDays;
        profile.SlotDurationMinutes = slotDurationMinutes;
        _db.SaveChanges();
    }

    // ── Queries ────────────────────────────────────────────────────────────────

    /// <summary>Returns all Users with role=Doctor, paired with their DoctorProfile.</summary>
    public List<(User User, DoctorProfile? Profile)> GetAllDoctors(UserService userService)
    {
        var doctors = userService.GetAllUsers()
            .Where(u => u.Role == UserRole.Doctor)
            .ToList();

        var profiles = _db.DoctorProfiles
            .Where(p => doctors.Select(d => d.Id).Contains(p.UserId))
            .ToList();

        return doctors.Select(d => (d, profiles.FirstOrDefault(p => p.UserId == d.Id))).ToList();
    }

    /// <summary>Returns the doctor with the most completed appointments in the current calendar month.</summary>
    public (User? Doctor, DoctorProfile? Profile, int AppointmentCount) GetBestDoctorOfMonth(
        UserService userService, AppointmentService appointmentService)
    {
        var from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var to   = from.AddMonths(1).AddTicks(-1);

        var doctors = userService.GetAllUsers().Where(u => u.Role == UserRole.Doctor).ToList();

        User? bestDoctor = null;
        DoctorProfile? bestProfile = null;
        int bestCount = 0;

        foreach (var doc in doctors)
        {
            var count = appointmentService.CountCompletedByDoctor(doc.Id, from, to);
            if (count > bestCount)
            {
                bestCount = count;
                bestDoctor = doc;
                bestProfile = GetByUserId(doc.Id);
            }
        }

        return (bestDoctor, bestProfile, bestCount);
    }
}
