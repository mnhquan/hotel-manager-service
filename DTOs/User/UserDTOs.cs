using System.ComponentModel.DataAnnotations;

namespace HotelManagement.API.DTOs.User;

public class UserProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Cccd { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Cccd { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
}
