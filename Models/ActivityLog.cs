namespace proekt.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = "Anonymous";
    public string Action { get; set; } = "";
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
