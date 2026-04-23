namespace proekt.Models;

public class DoctorProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Specialization { get; set; } = "General Practitioner";
    public string? Bio { get; set; }
    public decimal ConsultationFee { get; set; } = 50.00m;
    // Working hours stored as total minutes from midnight
    public int AvailableFromMinutes { get; set; } = 480;   // 08:00
    public int AvailableToMinutes { get; set; } = 1020;    // 17:00
    // Comma-separated day numbers: "1,2,3,4,5" = Mon-Fri
    public string WorkingDays { get; set; } = "1,2,3,4,5";
    public int SlotDurationMinutes { get; set; } = 30;
    public double AverageRating { get; set; } = 0;
    public int TotalRatings { get; set; } = 0;
}
