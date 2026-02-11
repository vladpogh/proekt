using proekt.Models;

namespace proekt.Services;

public class MedicalDocumentService
{
    private static List<MedicalDocument> _documents = new();

    public List<MedicalDocument> GetAllDocuments()
    {
        return _documents;
    }

    public List<MedicalDocument> GetPendingDocuments()
    {
        return _documents.Where(d => d.Status == DocumentStatus.Pending).ToList();
    }

    public void AddDocument(MedicalDocument doc)
    {
        doc.Id = _documents.Count > 0 ? _documents.Max(d => d.Id) + 1 : 1;
        _documents.Add(doc);
    }

    public void ApproveDocument(int id, string? comment = null)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == id);
        if (doc != null)
        {
            doc.Status = DocumentStatus.Approved;
            doc.Comment = comment;
        }
    }

    public void RejectDocument(int id, string? comment = null)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == id);
        if (doc != null)
        {
            doc.Status = DocumentStatus.Rejected;
            doc.Comment = comment;
        }
    }
}
