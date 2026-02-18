namespace proekt.Models
{
    public class ContactInquiry
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}