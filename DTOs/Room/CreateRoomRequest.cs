namespace HotelManagement.API.DTOs.Room;

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public int Capacity { get; set; }
    public int? BedCount { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? Area { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; } = "available";
    public int PropertyId { get; set; }
    public List<string>? ImageUrls { get; set; }
}
