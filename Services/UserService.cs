using proekt.Data;
using proekt.Models;
using Microsoft.EntityFrameworkCore;

namespace proekt.Services;

public class UserService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;

    public UserService(ApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public User? GetUserByEmail(string email)
    {
        return _db.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
    }

    public User? GetUserById(int id)
    {
        return _db.Users.Find(id);
    }

    public bool VerifyPassword(string email, string password)
    {
        var user = GetUserByEmail(email);
        return user != null && user.Password == password;
    }

    public bool VerifyPassword(int userId, string password)
    {
        var user = GetUserById(userId);
        return user != null && user.Password == password;
    }

    public async Task<bool> RegisterUser(string fullName, string email, string password, string verificationBaseUrl)
    {
        if (GetUserByEmail(email) != null)
            return false; // User already exists

        var token = Guid.NewGuid().ToString();
        var user = new User
        {
            FullName = fullName,
            Email = email,
            Password = password,
            CreatedAt = DateTime.Now,
            Role = UserRole.User,
            IsEmailVerified = false,
            VerificationToken = token
        };

        _db.Users.Add(user);
        _db.SaveChanges(); // Write to PostgreSQL

        var verificationLink = $"{verificationBaseUrl}?token={token}";
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e5e7eb; border-radius: 8px;'>
                <h2 style='color: #0052CC;'>Verify Your Email</h2>
                <p>Thank you for registering with MedReports. Please click the button below to verify your email address:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{verificationLink}' style='background-color: #0052CC; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Verify Email Address</a>
                </div>
                <p style='font-size: 0.9rem; color: #666;'>If the button doesn't work, copy and paste this link into your browser:</p>
                <p style='font-size: 0.8rem; color: #0052CC; word-break: break-all;'>{verificationLink}</p>
            </div>";

        await _emailService.SendEmailAsync(email, "Verify your email", emailBody);

        return true;
    }

    public bool VerifyEmail(string token)
    {
        var user = _db.Users.FirstOrDefault(u => u.VerificationToken == token);
        if (user == null) return false;

        user.IsEmailVerified = true;
        user.VerificationToken = null; // Clear token after verification
        _db.SaveChanges();
        return true;
    }

    public List<User> GetAllUsers()
    {
        return _db.Users.ToList();
    }

    public void UpdateUserRole(int userId, UserRole newRole)
    {
        var user = _db.Users.Find(userId);
        if (user != null)
        {
            user.Role = newRole;
            _db.SaveChanges(); // Write to PostgreSQL
        }
    }

    public bool ChangePassword(int userId, string newPassword)
    {
        var user = _db.Users.Find(userId);
        if (user == null) return false;
        user.Password = newPassword;
        _db.SaveChanges(); // Write to PostgreSQL
        return true;
    }

    public void RemoveUser(int userId)
    {
        var user = _db.Users.Find(userId);
        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges(); // Write to PostgreSQL
        }
    }

        public void UpdateUserProfile(int userId, string fullName, string? phone, string? location)
        {
            var user = _db.Users.Find(userId);
            if (user != null)
            {
                user.FullName = fullName;
                user.PhoneNumber = phone;
                user.Location = location;
                _db.SaveChanges(); // Write to PostgreSQL
            }
        }

        public int GetCountByRole(UserRole role)
        {
            return _db.Users.Count(u => u.Role == role);
        }
    }
