namespace HotelManagement.API.DTOs.Finance;

public class BusinessFinanceSummary
{
    public decimal TotalEarnings { get; set; }
    public decimal PendingPayouts { get; set; }
    public decimal AvailableBalance { get; set; }
    public List<FinanceTransactionDto> RecentTransactions { get; set; } = new();
}

public class AdminFinanceSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPayouts { get; set; }
    public int TotalBookings { get; set; }
    public int TotalUsers { get; set; }
    public int TotalProperties { get; set; }
    public List<FinanceTransactionDto> SystemTransactions { get; set; } = new();
}

public class FinanceTransactionDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}
