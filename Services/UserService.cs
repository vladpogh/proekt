using proekt.Data;
using proekt.Models;
using Microsoft.EntityFrameworkCore;

namespace proekt.Services;

public class UserService
{
    private readonly ApplicationDbContext _db;

    // ApplicationDbContext is injected by ASP.NET Core automatically
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

    public bool RegisterUser(string fullName, string email, string password)
    {
        if (GetUserByEmail(email) != null)
            return false; // User already exists

        _db.Users.Add(new User
        {
            FullName = fullName,
            Email = email,
            Password = password,
            CreatedAt = DateTime.Now,
            Role = UserRole.User
        });
        _db.SaveChanges(); // Write to PostgreSQL
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
