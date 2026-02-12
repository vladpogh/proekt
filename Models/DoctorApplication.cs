using System;

namespace proekt.Models;

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public class DoctorApplication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? About { get; set; }
    public string? EmploymentContractPath { get; set; }
    public string? IdCardPath { get; set; }
    public string? MedicalLicensePath { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
