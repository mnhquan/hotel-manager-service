using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.API.Models;

public class PartnerBankAccount
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public Partner? Partner { get; set; }
}
