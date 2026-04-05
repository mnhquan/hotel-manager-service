using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Room;
using HotelManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Services;

public interface IRoomService
{
    Task<List<RoomResponse>> GetAllRoomsAsync();
    Task<List<RoomResponse>> GetAdminRoomsAsync();
    Task<List<RoomResponse>> GetPartnerRoomsAsync(int partnerId);
    Task<RoomResponse?> GetRoomByIdAsync(int id);
    Task<(RoomResponse? room, string? error)> CreateRoomAsync(CreateRoomRequest req);
    Task<(RoomResponse? room, string? error)> UpdateRoomAsync(int id, UpdateRoomRequest req);
    Task<bool> ApproveRoomAsync(int id);
    Task<bool> RejectRoomAsync(int id);
    Task<bool> DeleteRoomAsync(int id);
}

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;

    public RoomService(AppDbContext db) => _db = db;

    public async Task<List<RoomResponse>> GetAllRoomsAsync()
    {
        var list = await _db.Rooms
            .Include(r => r.Property)
            .Include(r => r.Images)
            .Where(r => r.IsApproved == true && r.Status != "rejected" && r.Status != "deleted")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        
        return list.Select(MapToResponse).ToList();
    }

    public async Task<List<RoomResponse>> GetAdminRoomsAsync()
    {
        var list = await _db.Rooms
            .Include(r => r.Property)
            .Include(r => r.Images)
            .Where(r => r.Status != "deleted")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        
        return list.Select(MapToResponse).ToList();
    }

    public async Task<List<RoomResponse>> GetPartnerRoomsAsync(int partnerId)
    {
        try 
        {
            // Tìm ID đối tác thực tế từ UserId (partnerId) sử dụng LINQ
            Console.WriteLine($"[Service] GetPartnerRooms: Resolving partner record for UserId {partnerId}...");
            
            var partnerIds = await _db.Partners
                .Where(p => p.UserId == partnerId)
                .Select(p => p.Id)
                .ToListAsync();

            var partnerRecordId = partnerIds.FirstOrDefault();

            if (partnerRecordId == 0)
            {
                Console.WriteLine($"[Service] GetPartnerRooms: No doi_tac record found for UserId {partnerId}");
                return new List<RoomResponse>();
            }

            Console.WriteLine($"[Service] GetPartnerRooms: Found PartnerId {partnerRecordId}. Fetching property...");

            // Tìm cơ sở lưu trú của partner này trước
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.PartnerId == partnerRecordId);
            if (property == null) 
            {
                Console.WriteLine($"[Service] GetPartnerRooms: No property found for doi_tac_id {partnerRecordId} (User {partnerId})");
                return new List<RoomResponse>();
            }

            Console.WriteLine($"[Service] GetPartnerRooms: Found property '{property.Name}' (ID {property.Id}). Fetching rooms...");

            var list = await _db.Rooms
                .Include(r => r.Property)
                .Include(r => r.Images)
                .Where(r => r.PropertyId == property.Id && r.Status != "deleted")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            
            Console.WriteLine($"[Service] GetPartnerRooms: SUCCESS. Found {list.Count} rooms for PropertyId {property.Id}");
            return list.Select(MapToResponse).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Service] GetPartnerRooms ERROR: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            throw;
        }
    }

    public async Task<RoomResponse?> GetRoomByIdAsync(int id)
    {
        var room = await _db.Rooms
            .Include(r => r.Property)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);
            
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
            Status = "pending",
            IsApproved = false,
            CreatedAt = DateTime.Now
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        if (req.ImageUrls != null && req.ImageUrls.Any())
        {
            var images = req.ImageUrls.Select((url, index) => new RoomImage
            {
                RoomId = room.Id,
                Url = url,
                IsMainImage = index == 0,
                DisplayOrder = index,
                CreatedAt = DateTime.Now
            }).ToList();
            
            _db.RoomImages.AddRange(images);
            await _db.SaveChangesAsync();
        }

        // Fetch to include images
        var createdRoom = await _db.Rooms.Include(r => r.Property).Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == room.Id);

        return (MapToResponse(createdRoom ?? room), null);
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
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null) return false;

        bool hasBookingsOrReviews = await _db.Bookings.AnyAsync(b => b.RoomId == id) ||
                                     await _db.Reviews.AnyAsync(r => r.RoomId == id);
                                     
        if (hasBookingsOrReviews)
        {
            room.Status = "deleted";
            room.IsApproved = false; // Hide from public
            room.UpdatedAt = DateTime.Now;
        }
        else
        {
            // Clear from legacy tables not mapped in EF Core but existing in DB
            // Using raw SQL to satisfy foreign key constraints.
            // Wrap in try-catch to ignore errors if tables don't exist in different DB environments.
            try { 
                await _db.Database.ExecuteSqlRawAsync("DELETE FROM duyet_bai_dang WHERE phong_id = {0}", id);
                await _db.Database.ExecuteSqlRawAsync("DELETE FROM tien_nghi_phong WHERE phong_id = {0}", id);
            } catch { /* Table might not exist, ignore and continue */ }

            // Direct query for associated images by RoomId to ensure all are deleted
            // to satisfy foreign key constraints.
            var images = await _db.RoomImages.Where(i => i.RoomId == id).ToListAsync();
            if (images.Any())
            {
                _db.RoomImages.RemoveRange(images);
            }
            _db.Rooms.Remove(room);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveRoomAsync(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room is null) return false;

        room.IsApproved = true;
        room.Status = "approved"; // lowercase!
        room.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectRoomAsync(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room is null) return false;

        room.IsApproved = false;
        room.Status = "rejected"; // will be mapped to "Hidden" in UI
        room.UpdatedAt = DateTime.Now;

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
        UpdatedAt = r.UpdatedAt,
        // Lấy ảnh chính, nếu không có thì lấy ảnh đầu tiên
        MainImageUrl = r.Images?.FirstOrDefault(i => i.IsMainImage)?.Url ?? r.Images?.FirstOrDefault()?.Url,
        // Lấy toàn bộ danh sách ảnh cho gallery
        ImageUrls = r.Images?.OrderByDescending(i => i.IsMainImage).Select(i => i.Url).ToList() ?? new List<string>(),
        PropertyLocation = r.Property?.City,
        FullAddress = $"{r.Property?.DetailedAddress}, {r.Property?.Ward}, {r.Property?.District}, {r.Property?.City}",
        PropertyName = r.Property?.Name,
        PropertyDescription = r.Property?.Description,
        Rating = r.Property?.AverageRating
    };
}
