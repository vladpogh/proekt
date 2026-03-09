using proekt.Data;
using proekt.Models;

namespace proekt.Services;

public class DoctorApplicationService
{
    private readonly ApplicationDbContext _db;

    public DoctorApplicationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<DoctorApplication> GetAll()
    {
        return _db.DoctorApplications.ToList();
    }

    public List<DoctorApplication> GetPending()
    {
        return _db.DoctorApplications
                  .Where(a => a.Status == ApplicationStatus.Pending)
                  .ToList();
    }

    public DoctorApplication? GetById(int id)
    {
        return _db.DoctorApplications.Find(id);
    }

    public DoctorApplication? GetByUserId(int userId)
    {
        return _db.DoctorApplications
                  .Where(a => a.UserId == userId)
                  .OrderByDescending(a => a.CreatedAt)
                  .FirstOrDefault();
    }

    public void Add(DoctorApplication app)
    {
        _db.DoctorApplications.Add(app);
        _db.SaveChanges();
    }

    public void UpdateStatus(int id, ApplicationStatus status, string? adminComment = null)
    {
        var a = _db.DoctorApplications.Find(id);
        if (a != null)
        {
            a.Status = status;
            a.AdminComment = adminComment;
            _db.SaveChanges();
        }
    }
}
