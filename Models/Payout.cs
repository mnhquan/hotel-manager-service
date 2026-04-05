using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.API.Models;

public class Payout
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    
    public string Status { get; set; } = "pending"; // pending, paid, rejected
    
    public string? ProofImageUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? AdminNote { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation property
    public Partner? Partner { get; set; }
}
