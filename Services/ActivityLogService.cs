using proekt.Data;
using proekt.Models;

namespace proekt.Services;

public class ActivityLogService
{
    private readonly ApplicationDbContext _db;

    public ActivityLogService(ApplicationDbContext db) => _db = db;

    public void Log(int? userId, string userName, string action, string? details = null, string? ipAddress = null)
    {
        _db.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.Now
        });
        _db.SaveChanges();
    }

    public List<ActivityLog> GetAll()
        => _db.ActivityLogs
              .OrderByDescending(l => l.Timestamp)
              .ToList();

    public List<ActivityLog> GetByUser(int userId)
        => _db.ActivityLogs
              .Where(l => l.UserId == userId)
              .OrderByDescending(l => l.Timestamp)
              .ToList();

    public List<ActivityLog> GetRecent(int count)
        => _db.ActivityLogs
              .OrderByDescending(l => l.Timestamp)
              .Take(count)
              .ToList();

    public int GetTotalCount() => _db.ActivityLogs.Count();
}
