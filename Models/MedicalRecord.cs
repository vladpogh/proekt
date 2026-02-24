namespace proekt.Models;

public enum MedicalEntryType
{
    Operation,
    Diagnosis,
    Medication,
    Note
}

public class MedicalRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? GeneralNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<MedicalEntry> Entries { get; set; } = new();
}

public class MedicalEntry
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public MedicalEntryType Type { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public int CreatedByDoctorId { get; set; }
    public string CreatedByDoctorName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false; // soft-delete
}

public class MedicalAuditLog
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public int? EntryId { get; set; }
    public string ActionType { get; set; } = ""; // "AddEntry", "EditEntry", "UpdateRecord"
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Details { get; set; } = "";
}
