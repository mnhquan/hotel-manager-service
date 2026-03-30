namespace HotelManagement.API.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; }
    public int GuestCount { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal? CleaningFee { get; set; }
    public decimal? ServiceFee { get; set; }
    public decimal? Tax { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? Deposit { get; set; }
    public string? Status { get; set; } = "pending";
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
}

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public int? BookingId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
}

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string? PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public Booking Booking { get; set; } = null!;
}