using HotelManagementSystem_weapp.Data;
using HotelManagementSystem_weapp.Models;
using Microsoft.EntityFrameworkCore;

public class AuthService
{
    private readonly ApplicationDbContext _db;

    public AuthService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, ApplicationUser? User)> LoginAsync(string username, string password)
    {
        var user = await _db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return (false, null);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, null);

        return (true, user);
    }
}