using System.Text.Json.Serialization;

namespace HotelManagement.API.Models;

public class Property
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string DetailedAddress { get; set; } = string.Empty;
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public TimeSpan? DefaultCheckInTime { get; set; }
    public TimeSpan? DefaultCheckOutTime { get; set; }
    public string? Status { get; set; } = "active";
    public decimal? AverageRating { get; set; } = 0;
    public int? TotalReviews { get; set; } = 0;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public Partner Partner { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
