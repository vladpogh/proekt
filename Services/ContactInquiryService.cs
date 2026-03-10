using proekt.Data;
using proekt.Models;

namespace proekt.Services
{
    public class ContactInquiryService
    {
        private readonly ApplicationDbContext _db;

        public ContactInquiryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public void AddInquiry(ContactInquiry inquiry)
        {
            _db.ContactInquiries.Add(inquiry);
            _db.SaveChanges();
        }

        public List<ContactInquiry> GetAllInquiries()
        {
            return _db.ContactInquiries.OrderByDescending(i => i.CreatedAt).ToList();
        }

        public List<ContactInquiry> GetInquiriesByEmail(string email)
        {
            return _db.ContactInquiries
                      .Where(i => i.Email.ToLower() == email.ToLower())
                      .OrderByDescending(i => i.CreatedAt)
                      .ToList();
        }

        public ContactInquiry? GetInquiryById(int id)
        {
            return _db.ContactInquiries.Find(id);
        }

        public void RespondToInquiry(int id, string response)
        {
            var inquiry = _db.ContactInquiries.Find(id);
            if (inquiry != null)
            {
                inquiry.AdminResponse = response;
                _db.SaveChanges();
            }
        }
    }
}