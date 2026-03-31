using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Room;
using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Services;

public interface IRoomService
{
    Task<List<RoomResponse>> GetAllRoomsAsync();
    Task<RoomResponse?> GetRoomByIdAsync(int id);
    Task<(RoomResponse? room, string? error)> CreateRoomAsync(CreateRoomRequest req);
    Task<(RoomResponse? room, string? error)> UpdateRoomAsync(int id, UpdateRoomRequest req);
    Task<bool> DeleteRoomAsync(int id);
}

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;

    public RoomService(AppDbContext db) => _db = db;

    public async Task<List<RoomResponse>> GetAllRoomsAsync()
    {
        var list = await _db.Rooms
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        
        return list.Select(MapToResponse).ToList();
    }

    public async Task<RoomResponse?> GetRoomByIdAsync(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room is null) return null;
        
        return MapToResponse(room);
    }

    public async Task<(RoomResponse? room, string? error)> CreateRoomAsync(CreateRoomRequest req)
    {
        if (await _db.Rooms.AnyAsync(r => r.RoomCode == req.RoomCode))
        {
            return (null, "Mã phòng đã tồn tại");
        }

        var room = new Room
        {
            PropertyId = req.PropertyId,
            Name = req.Name,
            RoomCode = req.RoomCode,
            RoomType = req.RoomType,
            Capacity = req.Capacity,
            BedCount = req.BedCount,
            BasePrice = req.BasePrice,
            Area = req.Area,
            Description = req.Description,
            Status = req.Status ?? "available",
            IsApproved = true,
            CreatedAt = DateTime.Now
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return (MapToResponse(room), null);
    }

    public async Task<(RoomResponse? room, string? error)> UpdateRoomAsync(int id, UpdateRoomRequest req)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room is null)
            return (null, "Phòng không tồn tại");

        if (req.RoomCode is not null && req.RoomCode != room.RoomCode)
        {
            if (await _db.Rooms.AnyAsync(r => r.RoomCode == req.RoomCode))
                return (null, "Mã phòng mới đã tồn tại");
            
            room.RoomCode = req.RoomCode;
        }

        if (req.Name is not null) room.Name = req.Name;
        if (req.RoomType is not null) room.RoomType = req.RoomType;
        if (req.Capacity.HasValue) room.Capacity = req.Capacity.Value;
        if (req.BedCount.HasValue) room.BedCount = req.BedCount.Value;
        if (req.BasePrice.HasValue) room.BasePrice = req.BasePrice.Value;
        if (req.Area.HasValue) room.Area = req.Area.Value;
        if (req.Description is not null) room.Description = req.Description;
        if (req.Status is not null) room.Status = req.Status;
        if (req.IsApproved.HasValue) room.IsApproved = req.IsApproved.Value;
        if (req.PropertyId.HasValue) room.PropertyId = req.PropertyId.Value;

        room.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return (MapToResponse(room), null);
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room is null) return false;

        bool hasBookingsOrReviews = await _db.Bookings.AnyAsync(b => b.RoomId == id) ||
                                    await _db.Reviews.AnyAsync(r => r.RoomId == id);
                                    
        if (hasBookingsOrReviews)
        {
            room.Status = "deleted";
            room.UpdatedAt = DateTime.Now;
        }
        else
        {
            _db.Rooms.Remove(room);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    private static RoomResponse MapToResponse(Room r) => new()
    {
        Id = r.Id,
        PropertyId = r.PropertyId,
        Name = r.Name,
        RoomCode = r.RoomCode,
        RoomType = r.RoomType,
        Capacity = r.Capacity,
        BedCount = r.BedCount,
        BasePrice = r.BasePrice,
        Area = r.Area,
        Description = r.Description,
        Status = r.Status,
        IsApproved = r.IsApproved,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
