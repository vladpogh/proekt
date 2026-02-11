namespace proekt.Models;

public enum DocumentStatus
{
    Pending,
    Approved,
    Rejected
}

public class MedicalDocument
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? Comment { get; set; }
}
