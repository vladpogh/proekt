using proekt.Models;

namespace proekt.Services;

public class UserService
{
    private static List<User> _users = new()
    {
        new User
        {
            Id = 1,
            FullName = "Admin",
            Email = "admin@example.com",
            Password = "1234",
            CreatedAt = DateTime.Now,
            Role = UserRole.Admin
        },
        new User
        {
            Id = 2,
            FullName = "Manager",
            Email = "manager@example.com",
            Password = "1234",
            CreatedAt = DateTime.Now,
            Role = UserRole.Manager
        }
    };

    public User? GetUserByEmail(string email)
    {
        return _users.FirstOrDefault(u => u.Email?.ToLower() == email.ToLower());
    }

    public User? GetUserById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public bool VerifyPassword(string email, string password)
    {
        var user = GetUserByEmail(email);
        return user != null && user.Password == password;
    }

    public bool RegisterUser(string fullName, string email, string password)
    {
        if (GetUserByEmail(email) != null)
            return false; // User already exists

        var user = new User
        {
            Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
            FullName = fullName,
            Email = email,
            Password = password,
            CreatedAt = DateTime.Now,
            Role = UserRole.User // Default role
        };

        _users.Add(user);
        return true;
    }

    public List<User> GetAllUsers()
    {
        return _users;
    }

    public void UpdateUserRole(int userId, UserRole newRole)
    {
        var user = GetUserById(userId);
        if (user != null)
        {
            user.Role = newRole;
        }
    }

    public bool ChangePassword(int userId, string newPassword)
    {
        var user = GetUserById(userId);
        if (user == null) return false;
        user.Password = newPassword;
        return true;
    }

    public void RemoveUser(int userId)
    {
        var user = GetUserById(userId);
        if (user != null)
        {
            _users.Remove(user);
        }
    }
}
