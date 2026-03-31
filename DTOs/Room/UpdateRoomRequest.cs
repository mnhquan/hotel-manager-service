namespace HotelManagement.API.DTOs.Room;

public class UpdateRoomRequest
{
    public string? Name { get; set; }
    public string? RoomCode { get; set; }
    public string? RoomType { get; set; }
    public int? Capacity { get; set; }
    public int? BedCount { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? Area { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public bool? IsApproved { get; set; }
    public int? PropertyId { get; set; }
}
