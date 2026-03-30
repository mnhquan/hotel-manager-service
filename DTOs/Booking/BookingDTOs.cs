using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.DTOs.Booking;

public class CreateBookingRequest
{
    [Required] public int RoomId { get; set; }
    [Required] public DateTime CheckIn { get; set; }
    [Required] public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; } = 1;
    public string? Note { get; set; }
    public bool IsDeposit { get; set; } = true; // true = cọc 30%, false = full
}

public class BookingResponse
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? Deposit { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class OccupancyRateResponse
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public double OccupancyRate { get; set; }
    public int TotalDays { get; set; }
    public int OccupiedDays { get; set; }
}