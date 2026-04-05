using System.Security.Claims;
using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Room;
using HotelManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly AppDbContext _db;

    public RoomController(IRoomService roomService, AppDbContext db)
    {
        _roomService = roomService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("admin")]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAdminRooms()
    {
        var rooms = await _roomService.GetAdminRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("partner/{partnerId}")]
    // [Authorize(Roles = "partner")]
    public async Task<IActionResult> GetPartnerRooms(int partnerId)
    {
        Console.WriteLine($"[API] GetPartnerRooms requested for PartnerId: {partnerId}");
        var rooms = await _roomService.GetPartnerRoomsAsync(partnerId);
        Console.WriteLine($"[API] GetPartnerRooms returned {rooms.Count} rooms for PartnerId: {partnerId}");
        return Ok(rooms);
    }

    [HttpGet("my-listings")]
    [Authorize(Roles = "partner")]
    public async Task<IActionResult> GetMyListings()
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { message = "Không thể xác định danh tính đối tác" });
            }

            Console.WriteLine($"[API] GetMyListings for PartnerId: {userId}");
            var rooms = await _roomService.GetPartnerRoomsAsync(userId);
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] GetMyListings ERROR: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi lấy danh sách phòng cá nhân", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room is null) return NotFound(new { message = "Không tìm thấy phòng" });
        
        return Ok(room);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest req)
    {
        try
        {
            Console.WriteLine($"[API] CreateRoom attempt. Incoming PropertyId from JS: {req.PropertyId}");
            
            // Nếu là Partner, tự động tìm PropertyId của họ
            if (User.IsInRole("partner"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    // Tìm ID đối tác từ UserId (sử dụng model Partner mới)
                    var partner = await _db.Partners.FirstOrDefaultAsync(p => p.UserId == userId);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"[API] CreateRoom FAILED: No partner record found in DB for UserId {userId}");
                        return BadRequest(new { message = "Tài khoản của bạn chưa được liên kết với hồ sơ đối tác." });
                    }

                    var property = await _db.Properties.FirstOrDefaultAsync(p => p.PartnerId == partner.Id);
                    if (property == null) {
                        Console.WriteLine($"[API] CreateRoom FAILED: No property found in DB for doi_tac_id {partner.Id}");
                        return BadRequest(new { message = "Bạn chưa có cơ sở lưu trú. Vui lòng liên hệ Admin để khởi tạo khách sạn của bạn." });
                    }
                    req.PropertyId = property.Id;
                    Console.WriteLine($"[API] CreateRoom: Forced PropertyId to {property.Id} (Owner: Partner {partner.Id}/User {userId})");
                }
                else
                {
                    Console.WriteLine($"[API] CreateRoom FAILED: Could not parse NameIdentifier claim as int. Value: {userIdStr}");
                    return Unauthorized(new { message = "Thông tin xác thực không hợp lệ (Invalid ID)" });
                }
            }

            var (room, error) = await _roomService.CreateRoomAsync(req);
            if (error is not null) {
                Console.WriteLine($"[API] CreateRoom ERROR from Service: {error}");
                return BadRequest(new { message = error });
            }

            Console.WriteLine($"[API] CreateRoom SUCCESS: Room Id {room!.Id} [Code: {room.RoomCode}] created for PropertyId {room.PropertyId}");
            return CreatedAtAction(nameof(GetRoom), new { id = room!.Id }, room);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] CreateRoom FATAL: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, new { message = "Lỗi hệ thống khi tạo phòng. Vui lòng kiểm tra log backend." });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest req)
    {
        // Bảo vệ: Nếu là Partner, chỉ được sửa phòng thuộc Property của mình
        if (User.IsInRole("partner"))
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var partner = await _db.Partners.FirstOrDefaultAsync(p => p.UserId == userId);
                if (partner == null) return Forbid();

                var property = await _db.Properties.FirstOrDefaultAsync(p => p.PartnerId == partner.Id);
                if (property == null) return Forbid();
                
                // Kiểm tra xem phòng có thuộc property này không
                var existingRoom = await _db.Rooms.AnyAsync(r => r.Id == id && r.PropertyId == property.Id);
                if (!existingRoom) return StatusCode(403, new { message = "Bạn không có quyền chỉnh sửa phòng này" });
                
                req.PropertyId = property.Id; // Ép PropertyId cũ
            }
        }

        var (room, error) = await _roomService.UpdateRoomAsync(id, req);
        if (error is not null) return BadRequest(new { message = error });
        if (room is null) return NotFound(new { message = "Không tìm thấy phòng" });

        return Ok(room);
    }


    [HttpPut("{id}/approve")]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> ApproveRoom(int id)
    {
        var success = await _roomService.ApproveRoomAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy phòng" });

        return Ok(new { message = "Duyệt phòng thành công" });
    }

    [HttpPut("{id}/reject")]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> RejectRoom(int id)
    {
        var success = await _roomService.RejectRoomAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy phòng" });

        return Ok(new { message = "Đã từ chối phòng" });
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try 
        {
            var success = await _roomService.DeleteRoomAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy phòng" });

            return Ok(new { message = "Xóa phòng thành công" });
        }
        catch (Exception ex)
        {
            // Trả về lỗi chi tiết nhất từ database để debug
            var msg = ex.Message;
            var innerMsg = ex.InnerException?.Message ?? "";
            var innerInnerMsg = ex.InnerException?.InnerException?.Message ?? "";
            
            return StatusCode(500, new { 
                message = "Lỗi khi xóa phòng: " + msg,
                detail = string.Join(" | ", new[] { innerMsg, innerInnerMsg }.Where(s => !string.IsNullOrEmpty(s)))
            });
        }
    }
}
