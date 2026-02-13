using proekt.Models;

namespace proekt.Services
{
    public class ContactInquiryService
    {
        private static List<ContactInquiry> _inquiries = new();

        public void AddInquiry(ContactInquiry inquiry)
        {
            inquiry.Id = _inquiries.Count > 0 ? _inquiries.Max(i => i.Id) + 1 : 1;
            _inquiries.Add(inquiry);
        }

        public List<ContactInquiry> GetAllInquiries()
        {
            return _inquiries;
        }

        public ContactInquiry? GetInquiryById(int id)
        {
            return _inquiries.FirstOrDefault(i => i.Id == id);
        }

        public void RespondToInquiry(int id, string response)
        {
            var inquiry = GetInquiryById(id);
            if (inquiry != null)
            {
                inquiry.AdminResponse = response;
            }
        }
    }
}