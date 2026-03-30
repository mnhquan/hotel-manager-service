using System.Security.Claims;
using HotelManagement.API.DTOs.Booking;
using HotelManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    public BookingController(IBookingService bookingService) => _bookingService = bookingService;

    // POST /api/bookings
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (booking, error) = await _bookingService.CreateBookingAsync(userId.Value, req);
        if (error is not null) return BadRequest(new { message = error });

        return Ok(booking);
    }

    // GET /api/bookings/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var bookings = await _bookingService.GetUserBookingsAsync(userId.Value);
        return Ok(bookings);
    }

    // GET /api/bookings  (Admin only)
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    // PUT /api/bookings/{id}/confirm  (Admin only)
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var result = await _bookingService.ConfirmBookingAsync(id);
        if (result is null)
            return BadRequest(new { message = "Không thể xác nhận booking này" });

        return Ok(result);
    }

    // PUT /api/bookings/{id}/cancel
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var result = await _bookingService.CancelBookingAsync(id);
        if (result is null)
            return BadRequest(new { message = "Không thể hủy booking này" });

        return Ok(result);
    }

    // GET /api/bookings/occupancy?from=2024-01-01&to=2024-12-31  (Admin only)
    [HttpGet("occupancy")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOccupancyRates(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from >= to)
            return BadRequest(new { message = "Ngày bắt đầu phải trước ngày kết thúc" });

        var result = await _bookingService.GetOccupancyRatesAsync(from, to);
        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}