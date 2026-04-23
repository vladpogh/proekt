namespace proekt.Models;

public class Prescription
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string Medications { get; set; } = "";   // Free-text list of medications
    public string? Instructions { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.Now;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
