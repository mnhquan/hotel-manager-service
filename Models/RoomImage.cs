namespace HotelManagement.API.Models;

public class RoomImage
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMainImage { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public Room Room { get; set; } = null!;
}
