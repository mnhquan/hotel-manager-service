using HotelManagement.API.DTOs.Auth;
using HotelManagement.API.Services;
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

        var result = await _authService.RegisterCustomerAsync(req);
        if (result is null)
            return Conflict(new { message = "Email đã được sử dụng" });

        return Ok(result);
    }

    // POST /api/auth/register/partner
    [HttpPost("register/partner")]
    public async Task<IActionResult> RegisterPartner([FromBody] RegisterPartnerRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = ModelState });

        var result = await _authService.RegisterPartnerAsync(req);
        if (result is null)
            return Conflict(new { message = "Email đã được sử dụng" });

        return Ok(result);
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
}