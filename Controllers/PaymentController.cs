using HotelManagement.API.DTOs.Payment;
using HotelManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentController> _logger;

    public Regex BookingIdRegex = new Regex(@"BK(\d+)", RegexOptions.IgnoreCase);

    public PaymentController(
        IBookingService bookingService, 
        IConfiguration config,
        ILogger<PaymentController> logger)
    {
        _bookingService = bookingService;
        _config = config;
        _logger = logger;
    }

    [HttpPost("sepay-webhook")]
    public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookRequest request)
    {
        _logger.LogInformation("Received SePay Webhook: {Content}, Amount: {Amount}", request.BookingContent, request.AmountIn);

        // 1. Verify API Key (Authorization Header)
        var apiKey = _config["SePay:ApiKey"];
        var authHeader = Request.Headers["Authorization"].ToString();
        
        // SePay sends "Bearer <API_KEY>" or just the <API_KEY> depending on config
        if (!string.IsNullOrEmpty(apiKey) && !authHeader.Contains(apiKey))
        {
            _logger.LogWarning("Unauthorized SePay Webhook attempt. Header: {Header}", authHeader);
            return Unauthorized(new { message = "Invalid API Key" });
        }

        // 2. Extract Booking ID from Content (BK123)
        var match = BookingIdRegex.Match(request.BookingContent);
        if (!match.Success)
        {
            _logger.LogWarning("SePay Webhook content contains no Booking ID: {Content}", request.BookingContent);
            return Ok(new { message = "No booking ID found in content" });
        }

        int bookingId = int.Parse(match.Groups[1].Value);

        // 3. Confirm Booking
        var result = await _bookingService.ConfirmBookingAsync(bookingId);
        if (result == null)
        {
            _logger.LogWarning("Booking not found or already confirmed: {BookingId}", bookingId);
            return Ok(new { message = "Booking not found or already confirmed" });
        }

        _logger.LogInformation("Booking {BookingId} successfully confirmed via SePay Webhook", bookingId);
        
        return Ok(new { success = true, bookingId = bookingId });
    }
}
