using proekt.Data;
using proekt.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace proekt.Services;

public class UserService
{
    private readonly ApplicationDbContext _db;

    // BCrypt work factor: 12 is the recommended minimum for production (2^12 rounds)
    private const int BcryptWorkFactor = 12;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public User? GetUserByEmail(string email)
    {
        return _db.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
    }

    public User? GetUserById(int id)
    {
        return _db.Users.Find(id);
    }

    /// <summary>
    /// Verifies a plain-text password against the stored BCrypt hash.
    /// </summary>
    public bool VerifyPassword(string email, string password)
    {
        var user = GetUserByEmail(email);
        if (user == null || string.IsNullOrEmpty(user.Password)) return false;

        // BCrypt.Verify safely compares the plain-text password against the stored hash.
        // It is resistant to timing attacks and brute-force through the cost factor.
        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }

    /// <summary>
    /// Verifies a plain-text password against the stored BCrypt hash (by user ID).
    /// </summary>
    public bool VerifyPassword(int userId, string password)
    {
        var user = GetUserById(userId);
        if (user == null || string.IsNullOrEmpty(user.Password)) return false;
        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }

    /// <summary>
    /// Registers a new user, hashing the password with BCrypt before saving.
    /// </summary>
    public bool RegisterUser(string fullName, string email, string password)
    {
        if (GetUserByEmail(email) != null)
            return false; // User already exists

        // Hash the plain-text password before storing it in the database.
        // The hash includes an embedded random salt, so no two hashes are the same.
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BcryptWorkFactor);

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Password = hashedPassword,
            CreatedAt = DateTime.Now,
            Role = UserRole.User
        };

        _db.Users.Add(user);
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
            _db.SaveChanges();
        }
    }

    /// <summary>
    /// Changes the user's password, hashing the new value with BCrypt before saving.
    /// </summary>
    public bool ChangePassword(int userId, string newPassword)
    {
        var user = _db.Users.Find(userId);
        if (user == null) return false;

        // Always hash the new password before persisting it.
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword, BcryptWorkFactor);
        _db.SaveChanges();
        return true;
    }

    public void RemoveUser(int userId)
    {
        var user = _db.Users.Find(userId);
        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
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
            _db.SaveChanges();
        }
    }

    public int GetCountByRole(UserRole role)
    {
        return _db.Users.Count(u => u.Role == role);
    }
}
