using System.Security.Claims;
using HotelManagement.API.Data;
using HotelManagement.API.DTOs.Partner;
using HotelManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartnerController : ControllerBase
{
    private readonly AppDbContext _context;

    public PartnerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var partner = await _context.Partners
            .FirstOrDefaultAsync(p => p.UserId == userId.Value);

        if (partner == null) return NotFound("Partner not found");

        return Ok(partner);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdatePartnerProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var partner = await _context.Partners
            .FirstOrDefaultAsync(p => p.UserId == userId.Value);

        if (partner == null) return NotFound("Partner not found");

        partner.BusinessName = dto.BusinessName;
        partner.BusinessType = dto.BusinessType;
        partner.Address = dto.Address;
        partner.Description = dto.Description;
        partner.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(partner);
    }

    [HttpGet("bank-accounts")]
    public async Task<IActionResult> GetBankAccounts()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (partner == null) return NotFound();

        var accounts = await _context.PartnerBankAccounts
            .Where(a => a.PartnerId == partner.Id)
            .OrderByDescending(a => a.IsPrimary)
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpPost("bank-accounts")]
    public async Task<IActionResult> AddOrUpdateBankAccount(CreatePartnerBankAccountDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (partner == null) return NotFound();

        // Limit to 3 accounts per partner
        var count = await _context.PartnerBankAccounts.CountAsync(a => a.PartnerId == partner.Id);
        
        // Check if we are updating an existing one (simplification: if account number exists for this partner, update it)
        var existing = await _context.PartnerBankAccounts
            .FirstOrDefaultAsync(a => a.PartnerId == partner.Id && a.AccountNumber == dto.AccountNumber);

        if (existing != null)
        {
            existing.BankName = dto.BankName;
            existing.AccountHolder = dto.AccountHolder;
            existing.IsPrimary = dto.IsPrimary;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            if (count >= 3) return BadRequest("Tối đa chỉ được lưu 3 tài khoản ngân hàng.");
            
            var newAccount = new PartnerBankAccount
            {
                PartnerId = partner.Id,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                AccountHolder = dto.AccountHolder,
                IsPrimary = dto.IsPrimary,
                CreatedAt = DateTime.Now
            };
            _context.PartnerBankAccounts.Add(newAccount);
        }

        // If this is primary, unset other primaries
        if (dto.IsPrimary)
        {
            var otherPrimaries = await _context.PartnerBankAccounts
                .Where(a => a.PartnerId == partner.Id && a.AccountNumber != dto.AccountNumber && a.IsPrimary)
                .ToListAsync();
            foreach (var acc in otherPrimaries) acc.IsPrimary = false;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("bank-accounts/{id}")]
    public async Task<IActionResult> DeleteBankAccount(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (partner == null) return NotFound();

        var account = await _context.PartnerBankAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.PartnerId == partner.Id);

        if (account == null) return NotFound();

        _context.PartnerBankAccounts.Remove(account);
        await _context.SaveChangesAsync();
        return Ok();
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
