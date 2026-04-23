using Microsoft.EntityFrameworkCore;
using proekt.Models;

namespace proekt.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Existing tables ────────────────────────────────────────────────────────
    public DbSet<User> Users { get; set; }
    public DbSet<DoctorApplication> DoctorApplications { get; set; }
    public DbSet<ContactInquiry> ContactInquiries { get; set; }
    public DbSet<MedicalDocument> MedicalDocuments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<MedicalEntry> MedicalEntries { get; set; }
    public DbSet<MedicalAuditLog> MedicalAuditLogs { get; set; }

    // ── New tables ─────────────────────────────────────────────────────────────
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<DoctorProfile> DoctorProfiles { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Enum → String conversions ──────────────────────────────────────────
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<DoctorApplication>()
            .Property(d => d.Status)
            .HasConversion<string>();

        modelBuilder.Entity<MedicalDocument>()
            .Property(m => m.Status)
            .HasConversion<string>();

        modelBuilder.Entity<MedicalEntry>()
            .Property(e => e.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();

        // ── Navigation: MedicalRecord → MedicalEntries ─────────────────────────
        modelBuilder.Entity<MedicalRecord>()
            .HasMany(r => r.Entries)
            .WithOne()
            .HasForeignKey(e => e.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Decimal precision for Payment.Amount and DoctorProfile.ConsultationFee
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DoctorProfile>()
            .Property(d => d.ConsultationFee)
            .HasPrecision(18, 2);

        // ── Seed: default Admin and Manager users ─────────────────────────────
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin@example.com",
                Password = "1234",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Role = UserRole.Admin
            },
            new User
            {
                Id = 2,
                FullName = "Manager",
                Email = "manager@example.com",
                Password = "1234",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Role = UserRole.Manager
            }
        );
    }
}
