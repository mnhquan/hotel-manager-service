using System.Security.Claims;
using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Finance;
using HotelManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public FinanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("business")]
    public async Task<IActionResult> GetBusinessSummary()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        // 1. Tìm PartnerId ứng với UserId này
        var partnerId = await _context.Partners
            .Where(p => p.UserId == userId.Value)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        if (partnerId == 0) 
        {
            return Ok(new BusinessFinanceSummary()); // Trả về trống thay vì lỗi 404 để UI dễ xử lý
        }

        // 2. Lấy tất cả booking của các phòng thuộc sở hữu của Partner này (trừ các booking bị hủy)
        var bookings = await _context.Bookings
            .Include(b => b.Room).ThenInclude(r => r.Property)
            .Include(b => b.User)
            .Where(b => b.Room.Property.PartnerId == partnerId && b.Status != "Hủy" && b.Status != "Cancelled")
            .ToListAsync();

        // 3. Lấy các payout của Partner này
        var payouts = await _context.Payouts
            .Where(p => p.PartnerId == partnerId)
            .ToListAsync();

        var totalEarnings = bookings.Sum(b => (decimal?)b.TotalPrice) ?? 0;

        var pendingPayoutsTotal = payouts
            .Where(p => p.Status != null && p.Status.ToLower() == "pending")
            .Sum(p => p.Amount);

        var paidPayoutsTotal = payouts
            .Where(p => p.Status != null && p.Status.ToLower() == "paid")
            .Sum(p => p.Amount);

        var summary = new BusinessFinanceSummary
        {
            TotalEarnings = totalEarnings,
            PendingPayouts = pendingPayoutsTotal,
            AvailableBalance = totalEarnings - pendingPayoutsTotal - paidPayoutsTotal,
            
            RecentTransactions = bookings.OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new FinanceTransactionDto
                {
                    Id = b.Id,
                    RoomName = b.Room?.Name ?? "Phòng trống",
                    CustomerName = b.User?.FullName ?? "Ẩn danh",
                    Amount = b.TotalPrice,
                    Date = b.CreatedAt ?? DateTime.Now,
                    Status = b.Status ?? "unknown"
                }).ToList()
        };

        return Ok(summary);
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminSummary()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .ToListAsync();

        var totalRevenue = await _context.Bookings
            .Where(b => b.Status != "Hủy" && b.Status != "Cancelled")
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

        var totalPayouts = await _context.Payouts
            .Where(p => p.Status == "paid" || p.Status == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var recentBookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .Take(20)
            .ToListAsync();

        var summary = new AdminFinanceSummary
        {
            TotalRevenue = totalRevenue,
            TotalPayouts = totalPayouts,
            TotalBookings = await _context.Bookings.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            TotalProperties = await _context.Properties.CountAsync(),
            SystemTransactions = recentBookings.Select(b => new FinanceTransactionDto
            {
                Id = b.Id,
                RoomName = b.Room?.Name ?? "N/A",
                CustomerName = b.User?.FullName ?? "Unknown",
                Amount = b.TotalPrice,
                Date = b.CreatedAt ?? DateTime.Now,
                Status = b.Status ?? "unknown"
            }).ToList()
        };

        return Ok(summary);
    }

    [HttpPost("payout/request")]
    public async Task<IActionResult> CreatePayoutRequest(CreatePayoutRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (partner == null) return BadRequest("Đối tác không tồn tại.");

        // Kiểm tra số dư khả dụng (Tính trên tất cả booking không bị hủy)
        var totalEarnings = await _context.Bookings
            .Where(b => b.Room.Property.PartnerId == partner.Id && b.Status != "Hủy" && b.Status != "Cancelled")
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

        var deductedPayouts = await _context.Payouts
            .Where(p => p.PartnerId == partner.Id && (p.Status == "pending" || p.Status == "paid" || p.Status == "Pending" || p.Status == "Paid"))
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var availableBalance = totalEarnings - deductedPayouts;

        if (request.Amount > availableBalance)
        {
            return BadRequest($"Số dư không đủ. Số dư hiện tại là: {availableBalance:N0} VNĐ");
        }

        var payout = new Payout
        {
            PartnerId = partner.Id,
            Amount = request.Amount,
            BankName = request.BankName,
            AccountNumber = request.AccountNumber,
            AccountHolder = request.AccountHolder,
            Status = "pending",
            CreatedAt = DateTime.Now
        };

        _context.Payouts.Add(payout);
        await _context.SaveChangesAsync();

        return Ok(payout);
    }

    [HttpGet("payout/my-payouts")]
    public async Task<IActionResult> GetMyPayouts()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (partner == null) return BadRequest("Đối tác không tồn tại.");

        var payouts = await _context.Payouts
            .Where(p => p.PartnerId == partner.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payouts);
    }

    [HttpGet("payout/all")]
    public async Task<IActionResult> GetAllPayouts()
    {
        var payouts = await _context.Payouts
            .Include(p => p.Partner)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PayoutResponseDto
            {
                Id = p.Id,
                PartnerId = p.PartnerId,
                BusinessName = p.Partner != null ? p.Partner.BusinessName : "Unknown",
                Amount = p.Amount,
                BankName = p.BankName,
                AccountNumber = p.AccountNumber,
                AccountHolder = p.AccountHolder,
                Status = p.Status,
                ProofImageUrl = p.ProofImageUrl,
                AdminNote = p.AdminNote,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                PaidAt = p.PaidAt
            })
            .ToListAsync();

        return Ok(payouts);
    }

    [HttpPost("payout/{id}/process")]
    public async Task<IActionResult> ProcessPayout(int id, ProcessPayoutDto request)
    {
        var payout = await _context.Payouts.FindAsync(id);
        if (payout == null) return NotFound();

        payout.Status = request.Status;
        payout.AdminNote = request.AdminNote;
        payout.ProofImageUrl = request.ProofImageUrl;
        payout.UpdatedAt = DateTime.Now;

        if (request.Status.ToLower() == "paid")
        {
            payout.PaidAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return Ok(payout);
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
