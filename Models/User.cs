namespace proekt.Models;

public enum UserRole
{
    User,
    Doctor,
    Admin,
    Manager
}

public class User
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public UserRole Role { get; set; } = UserRole.User;
}
