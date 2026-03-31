using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Statistics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatisticsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// API 1: Tính tổng doanh thu (revenue)
    /// </summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueResponse>> GetTotalRevenue()
    {
        // Tính tổng TotalPrice của các Booking không bị hủy
        var total = await _context.Bookings
            .Where(b => b.Status != "Hủy" && b.Status != "Cancelled")
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

        return Ok(new RevenueResponse { TotalRevenue = total });
    }

    /// <summary>
    /// API 2: Tính tỷ lệ phòng được thuê trong hôm nay (occupancy rate)
    /// </summary>
    [HttpGet("occupancy-rate")]
    public async Task<ActionResult<OccupancyRateResponse>> GetOccupancyRate()
    {
        var totalRooms = await _context.Rooms.CountAsync();
        
        // Đếm các phòng đang có Booking bao trùm ngày hôm nay
        var today = DateTime.Today;
        var bookedRoomsCount = await _context.Bookings
            .Where(b => b.CheckIn <= today && b.CheckOut >= today && b.Status != "Hủy" && b.Status != "Cancelled")
            .Select(b => b.RoomId)
            .Distinct()
            .CountAsync();

        double percentage = totalRooms > 0 ? ((double)bookedRoomsCount / totalRooms) * 100 : 0;

        return Ok(new OccupancyRateResponse
        {
            Percentage = Math.Round(percentage, 2),
            TotalRooms = totalRooms,
            BookedRooms = bookedRoomsCount
        });
    }

    /// <summary>
    /// API 3: Lấy danh sách Top 5 phòng được thuê nhiều nhất
    /// </summary>
    [HttpGet("top-booked-rooms")]
    public async Task<ActionResult<IEnumerable<TopBookedRoomDto>>> GetTopBookedRooms()
    {
        var topRooms = await _context.Bookings
            .Where(b => b.Status != "Hủy" && b.Status != "Cancelled")
            .GroupBy(b => b.RoomId)
            .Select(g => new
            {
                RoomId = g.Key,
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(5)
            .Join(_context.Rooms,
                  b => b.RoomId,
                  r => r.Id,
                  (b, r) => new TopBookedRoomDto
                  {
                      RoomId = r.Id,
                      RoomName = r.Name,
                      BookingCount = b.BookingCount
                  })
            .ToListAsync();

        return Ok(topRooms);
    }

    /// <summary>
    /// API 4: Thuật toán đề xuất phòng cho người dùng
    /// Score = (0.6 * Rating) + (0.4 * BookingCount)
    /// Sắp xếp giảm dần theo Score
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<IEnumerable<RoomRecommendationDto>>> GetRoomRecommendations()
    {
        // Gộp query vào 1 lần chạy SQL bằng EF Core select projection
        var query = await _context.Rooms
            .Select(r => new
            {
                r.Id,
                r.Name,
                AverageRating = _context.Reviews.Where(rv => rv.RoomId == r.Id).Average(rv => (double?)rv.Stars) ?? 0.0,
                BookingCount = _context.Bookings.Where(b => b.RoomId == r.Id && b.Status != "Hủy" && b.Status != "Cancelled").Count()
            })
            .ToListAsync();

        // Xử lý tính toán tiếp Score trên RAM máy chủ sau khi query
        var recommendations = query.Select(x => new RoomRecommendationDto
        {
            RoomId = x.Id,
            RoomName = x.Name,
            AverageRating = Math.Round(x.AverageRating, 1),
            BookingCount = x.BookingCount,
            Score = Math.Round((0.6 * x.AverageRating) + (0.4 * x.BookingCount), 2)
        })
        .OrderByDescending(x => x.Score)
        .ToList();

        return Ok(recommendations);
    }
}
