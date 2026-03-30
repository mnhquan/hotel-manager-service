using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Booking;
using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Services;

public interface IBookingService
{
    Task<(BookingResponse? booking, string? error)> CreateBookingAsync(int userId, CreateBookingRequest req);
    Task<List<BookingResponse>> GetUserBookingsAsync(int userId);
    Task<List<BookingResponse>> GetAllBookingsAsync();
    Task<BookingResponse?> ConfirmBookingAsync(int bookingId);
    Task<BookingResponse?> CancelBookingAsync(int bookingId);
    Task<List<OccupancyRateResponse>> GetOccupancyRatesAsync(DateTime from, DateTime to);
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private const decimal DEPOSIT_RATE = 0.30m;

    public BookingService(AppDbContext db) => _db = db;

    public async Task<(BookingResponse? booking, string? error)> CreateBookingAsync(
        int userId, CreateBookingRequest req)
    {
        // Validate ngày
        if (req.CheckIn >= req.CheckOut)
            return (null, "Ngày check-in phải trước ngày check-out");

        if (req.CheckIn.Date < DateTime.Today)
            return (null, "Ngày check-in không được trong quá khứ");

        // Lấy phòng
        var room = await _db.Rooms.FindAsync(req.RoomId);
        if (room is null) return (null, "Phòng không tồn tại");
        if (room.Status == "maintenance")
            return (null, "Phòng đang bảo trì");

        // Thuật toán 4.2: Kiểm tra trùng lịch
        bool isOverlapping = await _db.Bookings.AnyAsync(b =>
            b.RoomId == req.RoomId &&
            b.Status != "cancelled" &&
            req.CheckIn < b.CheckOut &&
            req.CheckOut > b.CheckIn
        );
        if (isOverlapping)
            return (null, "Phòng đã được đặt trong khoảng thời gian này");

        // Thuật toán 4.3: Tính tổng tiền
        int nights = (int)(req.CheckOut - req.CheckIn).TotalDays;
        decimal totalPrice = room.BasePrice * nights;

        // Thuật toán 4.4: Tính tiền cọc
        decimal? deposit = req.IsDeposit
            ? Math.Round(totalPrice * DEPOSIT_RATE, 2)
            : totalPrice;

        var booking = new Booking
        {
            UserId = userId,
            RoomId = req.RoomId,
            CheckIn = req.CheckIn,
            CheckOut = req.CheckOut,
            Nights = nights,
            GuestCount = req.GuestCount,
            PricePerNight = room.BasePrice,
            TotalPrice = totalPrice,
            Deposit = deposit,
            Note = req.Note,
            Status = "pending",
            CreatedAt = DateTime.Now
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        await _db.Entry(booking).Reference(b => b.Room).LoadAsync();
        await _db.Entry(booking).Reference(b => b.User).LoadAsync();

        return (MapToResponse(booking), null);
    }

    public async Task<List<BookingResponse>> GetUserBookingsAsync(int userId)
    {
        var list = await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return list.Select(MapToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetAllBookingsAsync()
    {
        var list = await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return list.Select(MapToResponse).ToList();
    }

    public async Task<BookingResponse?> ConfirmBookingAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null || booking.Status != "pending") return null;

        booking.Status = "confirmed";
        booking.Room.Status = "occupied";
        await _db.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> CancelBookingAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null || booking.Status == "cancelled") return null;

        booking.Status = "cancelled";
        if (booking.Room.Status == "occupied")
            booking.Room.Status = "available";

        await _db.SaveChangesAsync();
        return MapToResponse(booking);
    }

    // Thuật toán 4.5: Tỷ lệ lấp phòng
    public async Task<List<OccupancyRateResponse>> GetOccupancyRatesAsync(DateTime from, DateTime to)
    {
        int totalDays = (int)(to - from).TotalDays;
        if (totalDays <= 0) return new();

        var rooms = await _db.Rooms.Include(r => r.Bookings).ToListAsync();

        return rooms.Select(room =>
        {
            int occupiedDays = room.Bookings
                .Where(b => b.Status != "cancelled")
                .Sum(b =>
                {
                    var start = b.CheckIn > from ? b.CheckIn : from;
                    var end = b.CheckOut < to ? b.CheckOut : to;
                    return start < end ? (int)(end - start).TotalDays : 0;
                });

            return new OccupancyRateResponse
            {
                RoomId = room.Id,
                RoomCode = room.RoomCode,
                RoomName = room.Name,
                OccupancyRate = Math.Round((double)occupiedDays / totalDays * 100, 2),
                TotalDays = totalDays,
                OccupiedDays = occupiedDays
            };
        }).ToList();
    }

    private static BookingResponse MapToResponse(Booking b) => new()
    {
        Id = b.Id,
        RoomId = b.RoomId,
        RoomName = b.Room.Name,
        RoomCode = b.Room.RoomCode,
        UserId = b.UserId,
        UserName = b.User.FullName,
        CheckIn = b.CheckIn,
        CheckOut = b.CheckOut,
        Nights = b.Nights,
        PricePerNight = b.PricePerNight,
        TotalPrice = b.TotalPrice,
        Deposit = b.Deposit,
        Status = b.Status,
        CreatedAt = b.CreatedAt
    };
}