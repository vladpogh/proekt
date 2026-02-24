using proekt.Models;

namespace proekt.Services;

public class MedicalRecordService
{
    private static List<MedicalRecord> _records = new();
    private static List<MedicalAuditLog> _auditLogs = new();
    private static int _nextRecordId = 1;
    private static int _nextEntryId = 1;
    private static int _nextLogId = 1;

    // ─── Records ───────────────────────────────────────────────────────────────

    public MedicalRecord CreateEmptyRecord(int userId)
    {
        // Guard: only one record per user
        if (_records.Any(r => r.UserId == userId))
            return _records.First(r => r.UserId == userId);

        var record = new MedicalRecord
        {
            Id = _nextRecordId++,
            UserId = userId,
            CreatedAt = DateTime.Now,
            Entries = new List<MedicalEntry>()
        };
        _records.Add(record);
        return record;
    }

    public MedicalRecord? GetRecordByUserId(int userId)
        => _records.FirstOrDefault(r => r.UserId == userId);

    public List<MedicalRecord> GetAllRecords() => _records;

    // ─── Entries ───────────────────────────────────────────────────────────────

    public MedicalEntry AddEntry(int userId, MedicalEntryType type, string title,
        string? description, DateTime date, int doctorId, string doctorName)
    {
        var record = GetRecordByUserId(userId)
                     ?? CreateEmptyRecord(userId);

        var entry = new MedicalEntry
        {
            Id = _nextEntryId++,
            MedicalRecordId = record.Id,
            Type = type,
            Title = title,
            Description = description,
            Date = date,
            CreatedByDoctorId = doctorId,
            CreatedByDoctorName = doctorName,
            CreatedAt = DateTime.Now
        };
        record.Entries.Add(entry);

        AddAuditLog(record.Id, entry.Id, "AddEntry", doctorId, doctorName,
            $"Added {type}: \"{title}\"");

        return entry;
    }

    public bool EditEntry(int entryId, MedicalEntryType type, string title,
        string? description, DateTime date, int doctorId, string doctorName)
    {
        foreach (var record in _records)
        {
            var entry = record.Entries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);
            if (entry == null) continue;

            entry.Type = type;
            entry.Title = title;
            entry.Description = description;
            entry.Date = date;

            AddAuditLog(record.Id, entryId, "EditEntry", doctorId, doctorName,
                $"Edited {type}: \"{title}\"");
            return true;
        }
        return false;
    }

    public MedicalEntry? GetEntryById(int entryId)
    {
        foreach (var record in _records)
        {
            var entry = record.Entries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);
            if (entry != null) return entry;
        }
        return null;
    }

    // ─── General record info update ────────────────────────────────────────────

    public void UpdateRecordInfo(int userId, string? bloodType, string? allergies,
        string? chronicConditions, string? generalNotes, int doctorId, string doctorName)
    {
        var record = GetRecordByUserId(userId);
        if (record == null) return;

        record.BloodType = bloodType;
        record.Allergies = allergies;
        record.ChronicConditions = chronicConditions;
        record.GeneralNotes = generalNotes;

        AddAuditLog(record.Id, null, "UpdateRecord", doctorId, doctorName,
            "Updated general record information");
    }

    // ─── Audit log ─────────────────────────────────────────────────────────────

    private void AddAuditLog(int recordId, int? entryId, string actionType,
        int doctorId, string doctorName, string details)
    {
        _auditLogs.Add(new MedicalAuditLog
        {
            Id = _nextLogId++,
            MedicalRecordId = recordId,
            EntryId = entryId,
            ActionType = actionType,
            DoctorId = doctorId,
            DoctorName = doctorName,
            Timestamp = DateTime.Now,
            Details = details
        });
    }

    public List<MedicalAuditLog> GetAuditLogs(int recordId)
        => _auditLogs.Where(l => l.MedicalRecordId == recordId)
                     .OrderByDescending(l => l.Timestamp)
                     .ToList();
}
