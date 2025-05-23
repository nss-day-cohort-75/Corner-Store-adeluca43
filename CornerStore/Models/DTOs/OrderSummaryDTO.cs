namespace CornerStore.Models.DTOs;

public class OrderSummaryDto
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public DateTime? PaidOnDate { get; set; }
}
