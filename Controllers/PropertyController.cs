using System.Security.Claims;
using HotelManagement.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertyController : ControllerBase
{
    private readonly AppDbContext _db;

    public PropertyController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyProperty()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này" });
        }

        try 
        {
            // Tìm bản ghi đối tác ứng với người dùng này (sử dụng LINQ thay vì SQL thô)
            Console.WriteLine($"[API] GetMyProperty: Resolving partner record for UserId {userId}...");
            
            var partnerIds = await _db.Partners
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            var partnerId = partnerIds.FirstOrDefault();

            if (partnerId == 0)
            {
                Console.WriteLine($"[API] GetMyProperty: No doi_tac record found for UserId {userId}");
                return NotFound(new { message = "Bạn chưa đăng ký thông tin đối tác" });
            }

            Console.WriteLine($"[API] GetMyProperty: Found PartnerId {partnerId}. Fetching property...");

            var property = await _db.Properties
                .FirstOrDefaultAsync(p => p.PartnerId == partnerId);
                
            if (property == null)
            {
                Console.WriteLine($"[API] GetMyProperty: No property found for doi_tac_id {partnerId}");
                return NotFound(new { message = "Không tìm thấy cơ sở lưu trú của bạn" });
            }

            Console.WriteLine($"[API] GetMyProperty: SUCCESS. Found property '{property.Name}' (ID {property.Id})");
            return Ok(property);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] GetMyProperty ERROR: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { message = "Lỗi hệ thống khi tải thông tin cơ sở", error = ex.Message });
        }
    }

    [HttpGet("partner")]
    public async Task<IActionResult> GetPartnerProperties()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện thao tác này" });
        }

        var partner = await _db.Partners.FirstOrDefaultAsync(p => p.UserId == userId);
        if (partner == null)
        {
            return NotFound(new { message = "Bạn chưa đăng ký thông tin đối tác" });
        }

        var properties = await _db.Properties
            .Where(p => p.PartnerId == partner.Id)
            .Include(p => p.Rooms)
            .ToListAsync();

        return Ok(properties);
    }
}
