namespace HotelManagement.API.Models;

public class Room
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public int Capacity { get; set; }
    public int? BedCount { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? Area { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; } = "available";
    public bool? IsApproved { get; set; } = false;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}