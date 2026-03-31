namespace HotelManagement.API.DTOs.Statistics;

public class RevenueResponse
{
    public decimal TotalRevenue { get; set; }
}

public class OccupancyRateResponse
{
    public double Percentage { get; set; }
    public int TotalRooms { get; set; }
    public int BookedRooms { get; set; }
}

public class TopBookedRoomDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

public class RoomRecommendationDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int BookingCount { get; set; }
    public double Score { get; set; }
}
