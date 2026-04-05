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
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest req);
    Task EnsureAdminUserAsync();
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
            throw new Exception("Email đã được sử dụng bởi một tài khoản khác.");
        
        if (await _db.Users.AnyAsync(u => u.Phone == req.Phone))
            throw new Exception("Số điện thoại đã được sử dụng bởi một tài khoản khác.");

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            Phone = req.Phone,
            Username = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Cccd = req.Cccd,
            Status = "active",
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Gán role customer
        await AssignRoleAsync(user.Id, "customer");

        return BuildAuthResponse(user, "customer");
    }

    public async Task<AuthResponse?> RegisterPartnerAsync(RegisterPartnerRequest req)
    {
        // Kiểm tra Email hoặc Số điện thoại đã tồn tại chưa
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
        {
            throw new Exception("Email đã được sử dụng bởi một tài khoản khác.");
        }
        if (await _db.Users.AnyAsync(u => u.Phone == req.Phone))
        {
            throw new Exception("Số điện thoại đã được sử dụng bởi một tài khoản khác.");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                Phone = req.Phone,
                Username = req.Email, // Email làm username mặc định (Phải có vì là Unique)
                Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Status = "active",
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            Console.WriteLine("[AuthService] Saving User...");
            await _db.SaveChangesAsync();

            // Gán role partner
            Console.WriteLine("[AuthService] Assigning Role...");
            await AssignRoleAsync(user.Id, "partner");

            // Tạo bản ghi đối tác (doi_tac) với mã định danh duy nhất để tránh lỗi Unique Constraint
            var partnerCode = "PARTNER-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var partner = new Partner
            {
                UserId = user.Id,
                PartnerCode = partnerCode,
                BusinessName = req.FullName,
                BusinessType = "Cá nhân",
                Status = "active",
                CreatedAt = DateTime.Now
            };
            _db.Partners.Add(partner);
            Console.WriteLine("[AuthService] Saving Partner...");
            await _db.SaveChangesAsync();

            // Tạo cơ sở lưu trú mặc định cho đối tác mới
            var property = new Property
            {
                PartnerId = partner.Id,
                Name = req.FullName,
                Type = "Khách sạn",
                City = "Chưa cập nhật",
                District = "Chưa cập nhật",
                Ward = "Chưa cập nhật",
                DetailedAddress = "Chưa cập nhật",
                Latitude = 0,
                Longitude = 0,
                DefaultCheckInTime = new TimeSpan(14, 0, 0), // 14:00
                DefaultCheckOutTime = new TimeSpan(12, 0, 0), // 12:00
                Status = "active",
                CreatedAt = DateTime.Now
            };

            _db.Properties.Add(property);
            Console.WriteLine("[AuthService] Saving Property...");
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            Console.WriteLine("[AuthService] Registration Committed Successfully.");
            return BuildAuthResponse(user, "partner");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"[AuthService] RegisterPartner FATAL EXCEPTION:\n{ex}");
            if (ex.InnerException != null) 
            {
                Console.WriteLine($"[InnerException]:\n{ex.InnerException}");
            }
            
            throw new Exception($"Đã xảy ra lỗi khi đăng ký đối tác (Xem log backend): {ex.Message}", ex);
        }
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

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest req)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.Password))
            throw new Exception("Mật khẩu hiện tại không chính xác.");

        user.Password = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTime.Now;
        
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task EnsureAdminUserAsync()
    {
        // Kiểm tra xem đã có admin nào chưa
        var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "admin");
        if (adminRole == null) 
        {
            adminRole = new Role { RoleName = "admin" };
            _db.Roles.Add(adminRole);
            await _db.SaveChangesAsync();
        }

        var adminExists = await _db.Users.AnyAsync(u => u.Email == "admin@staymaster.com");
        if (!adminExists)
        {
            var adminUser = new User
            {
                FullName = "System Administrator",
                Email = "admin@staymaster.com",
                Username = "admin@staymaster.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Status = "active",
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(adminUser);
            await _db.SaveChangesAsync();

            await AssignRoleAsync(adminUser.Id, "admin");
            Console.WriteLine("[AuthService] Default Admin created: admin@staymaster.com / Admin123!");
        }
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
            Avatar = user.Avatar,
            Role = role
        }
    };
}