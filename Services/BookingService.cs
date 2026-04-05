using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Booking;
using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HotelManagement.API.Services;

public interface IBookingService
{
    Task<(BookingResponse? booking, string? error)> CreateBookingAsync(int userId, CreateBookingRequest req);
    Task<List<BookingResponse>> GetUserBookingsAsync(int userId);
    Task<List<BookingResponse>> GetAllBookingsAsync();
    Task<BookingResponse?> GetBookingByIdAsync(int bookingId);
    Task<BookingResponse?> ConfirmBookingAsync(int bookingId);
    Task<BookingResponse?> CheckInAsync(int bookingId);
    Task<BookingResponse?> CompleteAsync(int bookingId);
    Task<BookingResponse?> CancelBookingAsync(int bookingId);
    Task<BookingResponse?> SyncSePayPaymentAsync(int bookingId);
    Task<List<BookingResponse>> GetPartnerBookingsAsync(int partnerId);
    Task<List<BookingResponse>> GetPartnerBookingsByUserIdAsync(int userId);
    Task<List<OccupancyRateResponse>> GetOccupancyRatesAsync(DateTime from, DateTime to);
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private const decimal DEPOSIT_RATE = 0.30m;

    public BookingService(AppDbContext db, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

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
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return list.Select(MapToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetAllBookingsAsync()
    {
        var list = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return list.Select(MapToResponse).ToList();
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse?> ConfirmBookingAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null || !string.Equals(booking.Status, "pending", StringComparison.OrdinalIgnoreCase)) return null;

        booking.Status = "confirmed";
        // booking.Room.Status = "occupied"; // Don't set occupied yet, wait for check-in
        await _db.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> CheckInAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null || !string.Equals(booking.Status, "confirmed", StringComparison.OrdinalIgnoreCase)) return null;

        booking.Status = "checked_in";
        booking.Room.Status = "occupied";
        await _db.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> CompleteAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(r => r.Room).ThenInclude(p => p.Property).ThenInclude(ptr => ptr.Partner)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null || !string.Equals(booking.Status, "checked_in", StringComparison.OrdinalIgnoreCase)) return null;

        booking.Status = "completed";
        booking.Room.Status = "available";
        
        // Update Partner WalletBalance
        var partner = booking.Room.Property.Partner;
        if (partner != null)
        {
            partner.WalletBalance += booking.TotalPrice;
        }

        await _db.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> CancelBookingAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null || string.Equals(booking.Status, "cancelled", StringComparison.OrdinalIgnoreCase)) return null;

        booking.Status = "cancelled";
        if (booking.Room.Status == "occupied")
            booking.Room.Status = "available";

        await _db.SaveChangesAsync();
        return MapToResponse(booking);
    }

    // Thuật toán 4.5: Tỷ lệ lấp phòng
    public async Task<BookingResponse?> SyncSePayPaymentAsync(int bookingId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return null;
        if (booking.Status?.ToLower() == "confirmed") return MapToResponse(booking);

        // 1. Prepare SePay API Call
        var apiKey = _config["SePay:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return MapToResponse(booking);

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            // SePay API v2 - correct URL and use search query for BK{id}
            var url = $"https://userapi.sepay.vn/v2/transactions?page=1&per_page=20&transaction_date_sort=desc&q=BK{bookingId}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return MapToResponse(booking);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            // SePay v2 response: { "status": "success", "data": [...] }
            if (doc.RootElement.TryGetProperty("data", out var transactions))
            {
                foreach (var tx in transactions.EnumerateArray())
                {
                    // SePay v2 field names
                    string content = "";
                    if (tx.TryGetProperty("transaction_content", out var contentEl))
                        content = contentEl.GetString() ?? "";

                    // amount_in = incoming transfer amount
                    decimal amountIn = 0;
                    if (tx.TryGetProperty("amount_in", out var amountEl))
                        amountIn = amountEl.GetDecimal();

                    // transfer_type = "in" means money received
                    string transferType = "";
                    if (tx.TryGetProperty("transfer_type", out var typeEl))
                        transferType = typeEl.GetString() ?? "";

                    // Match: content contains BK{id} AND it's an incoming transfer
                    bool contentMatch = content.Contains($"BK{bookingId}", StringComparison.OrdinalIgnoreCase);
                    bool isIncoming = transferType == "in" || amountIn > 0;

                    if (contentMatch && isIncoming)
                    {
                        return await ConfirmBookingAsync(bookingId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SePay Sync Error] BK{bookingId}: {ex.Message}");
        }

        return MapToResponse(booking);
    }

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

    public async Task<List<BookingResponse>> GetPartnerBookingsAsync(int partnerId)
    {
        var list = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Images)
            .Include(b => b.User)
            .Where(b => b.Room.Property.PartnerId == partnerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return list.Select(MapToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetPartnerBookingsByUserIdAsync(int userId)
    {
        var partner = await _db.Partners.FirstOrDefaultAsync(p => p.UserId == userId);
        if (partner == null) return new List<BookingResponse>();
        
        return await GetPartnerBookingsAsync(partner.Id);
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
        RoomImage = b.Room.Images.FirstOrDefault(i => i.IsMainImage)?.Url 
                    ?? b.Room.Images.FirstOrDefault()?.Url,
        UserAvatar = b.User.Avatar,
        CreatedAt = b.CreatedAt
    };
}