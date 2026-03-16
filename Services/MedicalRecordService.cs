using proekt.Data;
using proekt.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace proekt.Services;

public class MedicalRecordService
{
    private readonly ApplicationDbContext _db;

    public MedicalRecordService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ─── Records ───────────────────────────────────────────────────────────────

    public MedicalRecord CreateEmptyRecord(int userId)
    {
        // Guard: only one record per user
        var existing = _db.MedicalRecords.FirstOrDefault(r => r.UserId == userId);
        if (existing != null) return existing;

        var record = new MedicalRecord
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            Entries = new List<MedicalEntry>()
        };
        _db.MedicalRecords.Add(record);
        _db.SaveChanges();
        return record;
    }

    public MedicalRecord? GetRecordByUserId(int userId)
        => _db.MedicalRecords
              .Include(r => r.Entries.Where(e => !e.IsDeleted))
              .FirstOrDefault(r => r.UserId == userId);

    public List<MedicalRecord> GetAllRecords()
        => _db.MedicalRecords
              .Include(r => r.Entries.Where(e => !e.IsDeleted))
              .ToList();

    // ─── Entries ───────────────────────────────────────────────────────────────

    public MedicalEntry AddEntry(int userId, MedicalEntryType type, string title,
        string? description, DateTime date, int doctorId, string doctorName)
    {
        var record = GetRecordByUserId(userId)
                     ?? CreateEmptyRecord(userId);

        var entry = new MedicalEntry
        {
            MedicalRecordId = record.Id,
            Type = type,
            Title = title,
            Description = description,
            Date = date,
            CreatedByDoctorId = doctorId,
            CreatedByDoctorName = doctorName,
            CreatedAt = DateTime.Now
        };
        _db.MedicalEntries.Add(entry);
        _db.SaveChanges();

        AddAuditLog(record.Id, entry.Id, "AddEntry", doctorId, doctorName,
            $"Added {type}: \"{title}\"");

        return entry;
    }

    public bool EditEntry(int entryId, MedicalEntryType type, string title,
        string? description, DateTime date, int doctorId, string doctorName)
    {
        var entry = _db.MedicalEntries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);
        if (entry == null) return false;

        entry.Type = type;
        entry.Title = title;
        entry.Description = description;
        entry.Date = date;
        _db.SaveChanges();

        AddAuditLog(entry.MedicalRecordId, entryId, "EditEntry", doctorId, doctorName,
            $"Edited {type}: \"{title}\"");
        return true;
    }

    public MedicalEntry? GetEntryById(int entryId)
        => _db.MedicalEntries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);

    // ─── General record info update ────────────────────────────────────────────

    public void UpdateRecordInfo(int userId, string? bloodType, string? allergies,
        string? chronicConditions, string? generalNotes, int actorId, string actorName)
    {
        var record = _db.MedicalRecords.FirstOrDefault(r => r.UserId == userId);
        if (record == null) return;

        // Capture before-state
        var oldSnap = JsonSerializer.Serialize(new
        {
            BloodType        = record.BloodType,
            Allergies        = record.Allergies,
            ChronicConditions = record.ChronicConditions,
            GeneralNotes     = record.GeneralNotes
        });

        record.BloodType          = bloodType;
        record.Allergies          = allergies;
        record.ChronicConditions  = chronicConditions;
        record.GeneralNotes       = generalNotes;
        _db.SaveChanges();

        // Capture after-state
        var newSnap = JsonSerializer.Serialize(new
        {
            BloodType        = record.BloodType,
            Allergies        = record.Allergies,
            ChronicConditions = record.ChronicConditions,
            GeneralNotes     = record.GeneralNotes
        });

        AddAuditLogWithSnapshot(record.Id, null, "UpdateRecord", actorId, actorName,
            "Updated general record information", oldSnap, newSnap);
    }

    // ─── Audit log ─────────────────────────────────────────────────────────────

    private void AddAuditLog(int recordId, int? entryId, string actionType,
        int doctorId, string doctorName, string details)
    {
        _db.MedicalAuditLogs.Add(new MedicalAuditLog
        {
            MedicalRecordId = recordId,
            EntryId = entryId,
            ActionType = actionType,
            DoctorId = doctorId,
            DoctorName = doctorName,
            Timestamp = DateTime.Now,
            Details = details
        });
        _db.SaveChanges();
    }

    private void AddAuditLogWithSnapshot(int recordId, int? entryId, string actionType,
        int actorId, string actorName, string details, string? oldSnapshot, string? newSnapshot)
    {
        _db.MedicalAuditLogs.Add(new MedicalAuditLog
        {
            MedicalRecordId = recordId,
            EntryId         = entryId,
            ActionType      = actionType,
            DoctorId        = actorId,
            DoctorName      = actorName,
            Timestamp       = DateTime.Now,
            Details         = details,
            OldSnapshot     = oldSnapshot,
            NewSnapshot     = newSnapshot
        });
        _db.SaveChanges();
    }

    public List<MedicalAuditLog> GetAuditLogs(int recordId)
        => _db.MedicalAuditLogs
              .Where(l => l.MedicalRecordId == recordId)
              .OrderByDescending(l => l.Timestamp)
              .ToList();

    public int GetTotalRecordsCount()
    {
        return _db.MedicalRecords.Count();
    }
}
