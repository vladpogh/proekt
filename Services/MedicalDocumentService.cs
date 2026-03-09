using proekt.Data;
using proekt.Models;

namespace proekt.Services;

public class MedicalDocumentService
{
    private readonly ApplicationDbContext _db;

    public MedicalDocumentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<MedicalDocument> GetAllDocuments()
    {
        return _db.MedicalDocuments.ToList();
    }

    public List<MedicalDocument> GetPendingDocuments()
    {
        return _db.MedicalDocuments
                  .Where(d => d.Status == DocumentStatus.Pending)
                  .ToList();
    }

    public void AddDocument(MedicalDocument doc)
    {
        _db.MedicalDocuments.Add(doc);
        _db.SaveChanges();
    }

    public void ApproveDocument(int id, string? comment = null)
    {
        var doc = _db.MedicalDocuments.Find(id);
        if (doc != null)
        {
            doc.Status = DocumentStatus.Approved;
            doc.Comment = comment;
            _db.SaveChanges();
        }
    }

    public void RejectDocument(int id, string? comment = null)
    {
        var doc = _db.MedicalDocuments.Find(id);
        if (doc != null)
        {
            doc.Status = DocumentStatus.Rejected;
            doc.Comment = comment;
            _db.SaveChanges();
        }
    }
}
