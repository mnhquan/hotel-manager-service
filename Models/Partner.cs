using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HotelManagement.API.Models;

public class Partner
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? PartnerCode { get; set; }
    public string? BusinessName { get; set; }
    public string? BusinessType { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; } = "active";
    public decimal WalletBalance { get; set; } = 0;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
    
    [JsonIgnore]
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
