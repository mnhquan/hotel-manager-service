using System.Security.Claims;
using HotelManagement.API.DTOs.Auth;
using HotelManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    // POST /api/auth/register/customer
    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = ModelState });

        try {
            var result = await _authService.RegisterCustomerAsync(req);
            return Ok(result);
        } catch (Exception ex) {
            return Conflict(new { message = ex.Message });
        }
    }

    // POST /api/auth/register/partner
    [HttpPost("register/partner")]
    public async Task<IActionResult> RegisterPartner([FromBody] RegisterPartnerRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = ModelState });

        try {
            var result = await _authService.RegisterPartnerAsync(req);
            return Ok(result);
        } catch (Exception ex) {
            // AuthService hiện tại ném Exception cho trường hợp conflict (Email/Phone tồn tại)
            return Conflict(new { message = ex.Message });
        }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

        var result = await _authService.LoginAsync(req);
        if (result is null)
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

        return Ok(result);
    }

    // POST /api/auth/change-password
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

        try {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng" });

            var result = await _authService.ChangePasswordAsync(userId, req);
            if (result)
                return Ok(new { message = "Đổi mật khẩu thành công" });
            
            return BadRequest(new { message = "Không thể đổi mật khẩu" });
        } catch (Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
    }
}