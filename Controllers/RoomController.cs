using HotelManagement.API.DTOs.Room;
using HotelManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room is null) return NotFound(new { message = "Không tìm thấy phòng" });
        
        return Ok(room);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest req)
    {
        var (room, error) = await _roomService.CreateRoomAsync(req);
        if (error is not null) return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetRoom), new { id = room!.Id }, room);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest req)
    {
        var (room, error) = await _roomService.UpdateRoomAsync(id, req);
        if (error is not null) return BadRequest(new { message = error });
        if (room is null) return NotFound(new { message = "Không tìm thấy phòng" });

        return Ok(room);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var success = await _roomService.DeleteRoomAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy phòng" });

        return Ok(new { message = "Xóa phòng thành công" });
    }
}
