using proekt.Models;

namespace proekt.Services;

public class DoctorApplicationService
{
    private static List<DoctorApplication> _applications = new();

    public List<DoctorApplication> GetAll()
    {
        return _applications;
    }

    public List<DoctorApplication> GetPending()
    {
        return _applications.Where(a => a.Status == ApplicationStatus.Pending).ToList();
    }

    public DoctorApplication? GetById(int id)
    {
        return _applications.FirstOrDefault(a => a.Id == id);
    }

    public DoctorApplication? GetByUserId(int userId)
    {
        return _applications.OrderByDescending(a => a.CreatedAt).FirstOrDefault(a => a.UserId == userId);
    }

    public void Add(DoctorApplication app)
    {
        app.Id = _applications.Count > 0 ? _applications.Max(a => a.Id) + 1 : 1;
        _applications.Add(app);
    }

    public void UpdateStatus(int id, ApplicationStatus status, string? adminComment = null)
    {
        var a = GetById(id);
        if (a != null)
        {
            a.Status = status;
            a.AdminComment = adminComment;
        }
    }
}
