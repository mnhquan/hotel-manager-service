using System;

namespace HotelManagement.API.DTOs.Finance;

public class CreatePayoutRequestDto
{
    public decimal Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
}

public class PayoutResponseDto
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public string? BusinessName { get; set; }
    public decimal Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string? ProofImageUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class ProcessPayoutDto
{
    public string Status { get; set; } = "paid"; // paid, rejected
    public string? AdminNote { get; set; }
    public string? TransactionId { get; set; }
    public string? ProofImageUrl { get; set; }
}
