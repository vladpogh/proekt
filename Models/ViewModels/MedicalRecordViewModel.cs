namespace proekt.Models.ViewModels;

public class MedicalRecordViewModel
{
    public User Patient { get; set; } = null!;
    public MedicalRecord Record { get; set; } = null!;
    public List<MedicalAuditLog> AuditLogs { get; set; } = new();
    public bool CanEdit { get; set; } = false; // Doctor or Admin
}

public class AddMedicalEntryViewModel
{
    public int UserId { get; set; }
    public MedicalEntryType Type { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
}

public class UpdateMedicalRecordViewModel
{
    public int UserId { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? GeneralNotes { get; set; }
}

public class DoctorDashboardViewModel
{
    public List<User> Patients { get; set; } = new();
    public string? SearchQuery { get; set; }
}
