using System.Security.Claims;
using HotelManagement.API.DTOs.User;
using HotelManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
            return Unauthorized();

        var profile = await _userService.GetUserProfileAsync(userId);
        if (profile == null) return NotFound(new { message = "Người dùng không tồn tại" });

        return Ok(profile);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
            return Unauthorized();

        var updatedProfile = await _userService.UpdateProfileAsync(userId, req);
        if (updatedProfile == null) return NotFound(new { message = "Người dùng không tồn tại" });

        return Ok(updatedProfile);
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(currentUserIdString, out int currentUserId) && currentUserId == id)
        {
            return BadRequest(new { message = "Không thể xóa tài khoản của chính bạn đang đăng nhập" });
        }

        var success = await _userService.DeleteUserAsync(id);
        if (!success) return NotFound(new { message = "Người dùng không tồn tại" });

        return Ok(new { message = "Đã xóa người dùng thành công" });
    }
}
