using HotelManagement.API.Data;
using HotelManagement.API.DTOs.User;
using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Services;

public interface IUserService
{
    Task<List<UserProfileResponse>> GetAllUsersAsync();
    Task<UserProfileResponse?> GetUserProfileAsync(int userId);
    Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest req);
    Task<bool> DeleteUserAsync(int id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<List<UserProfileResponse>> GetAllUsersAsync()
    {
        var users = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return null;
        return MapToResponse(user);
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest req)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return null;

        user.FullName = req.FullName;
        user.Phone = req.Phone;
        user.Cccd = req.Cccd;
        user.Address = req.Address;
        user.Avatar = req.Avatar;
        user.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        // Don't delete self logic handled by controller if needed
        var user = await _db.Users.FindAsync(id);
        if (user is null) return false;

        // Xóa related UserRoles manually to prevent cascade issues
        var userRoles = await _db.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
        _db.UserRoles.RemoveRange(userRoles);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    private static UserProfileResponse MapToResponse(User user)
    {
        var role = user.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? "customer";
        
        return new UserProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Cccd = user.Cccd,
            Address = user.Address,
            Avatar = user.Avatar,
            Status = user.Status ?? "active",
            Role = role,
            CreatedAt = user.CreatedAt ?? DateTime.Now
        };
    }
}
