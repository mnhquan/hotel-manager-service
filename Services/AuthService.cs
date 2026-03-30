using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Auth;
using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterCustomerAsync(RegisterCustomerRequest req);
    Task<AuthResponse?> RegisterPartnerAsync(RegisterPartnerRequest req);
    Task<AuthResponse?> LoginAsync(LoginRequest req);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthService(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponse?> RegisterCustomerAsync(RegisterCustomerRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return null;

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            Phone = req.Phone,
            Username = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Cccd = req.Cccd,
            Status = "active"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Gán role customer
        await AssignRoleAsync(user.Id, "customer");

        return BuildAuthResponse(user, "customer");
    }

    public async Task<AuthResponse?> RegisterPartnerAsync(RegisterPartnerRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return null;

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            Phone = req.Phone,
            Username = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Cccd = req.Cccd,
            Status = "active"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Gán role partner
        await AssignRoleAsync(user.Id, "partner");

        return BuildAuthResponse(user, "partner");
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        // Lấy user kèm role (join qua bảng nguoi_dung_vai_tro)
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
            return null;

        // Lấy role đầu tiên (ưu tiên admin > partner > customer)
        var role = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .OrderByDescending(r => r == "admin" ? 3 : r == "partner" ? 2 : 1)
            .FirstOrDefault() ?? "customer";

        return BuildAuthResponse(user, role);
    }

    // ── Helpers ───────────────────────────────────────────────
    private async Task AssignRoleAsync(int userId, string roleName)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        if (role is null) return;

        _db.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = role.Id
        });
        await _db.SaveChangesAsync();
    }

    private AuthResponse BuildAuthResponse(User user, string role) => new()
    {
        Token = _jwt.GenerateToken(user, role),
        User = new UserInfo
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = role
        }
    };
}