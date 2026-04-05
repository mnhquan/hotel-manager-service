namespace HotelManagement.API.DTOs.Room;

public class RoomResponse
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
    public string? Status { get; set; }
    public bool? IsApproved { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Dữ liệu mở rộng từ các bảng Join
    public string? MainImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? PropertyLocation { get; set; } // Tỉnh/TP
    public string? FullAddress { get; set; }      // Toàn bộ địa chỉ
    public string? PropertyName { get; set; }
    public string? PropertyDescription { get; set; }
    public decimal? Rating { get; set; }
}
