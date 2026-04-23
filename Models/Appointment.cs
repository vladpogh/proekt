namespace proekt.Models;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? PatientNotes { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
