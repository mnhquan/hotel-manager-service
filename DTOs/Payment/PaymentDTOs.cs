using System.Text.Json.Serialization;

namespace HotelManagement.API.DTOs.Payment;

public class SePayWebhookRequest
{
    public long? id { get; set; }
    public string? gateway { get; set; }
    public string? transactionDate { get; set; }
    public string? accountNumber { get; set; }
    public decimal amount { get; set; }
    public string? content { get; set; }
    public string? transferType { get; set; }
    public decimal transferAmount { get; set; }
    public decimal accumulated { get; set; }
    public string? subAccount { get; set; }
    public string? bankCode { get; set; }
    public string? description { get; set; }
    public string? referenceNumber { get; set; }

    // Helpers to support multiple versions/names
    [JsonIgnore]
    public string BookingContent => content ?? description ?? "";
    
    [JsonIgnore]
    public decimal AmountIn => transferAmount > 0 ? transferAmount : amount;
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
