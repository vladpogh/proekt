using Microsoft.EntityFrameworkCore;
using proekt.Models;

namespace proekt.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Each DbSet maps to one table in SQL Server
    public DbSet<User> Users { get; set; }
    public DbSet<DoctorApplication> DoctorApplications { get; set; }
    public DbSet<ContactInquiry> ContactInquiries { get; set; }
    public DbSet<MedicalDocument> MedicalDocuments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<MedicalEntry> MedicalEntries { get; set; }
    public DbSet<MedicalAuditLog> MedicalAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Enum → String conversions (more readable in DB than integers) ---
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

        // --- Navigation: MedicalRecord has many MedicalEntries ---
        modelBuilder.Entity<MedicalRecord>()
            .HasMany(r => r.Entries)
            .WithOne()
            .HasForeignKey(e => e.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Seed: default Admin and Manager users ---
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
