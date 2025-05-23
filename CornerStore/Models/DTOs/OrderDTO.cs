
namespace CornerStore.Models.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string CashierId { get; set; }

    public string CashierName { get; set; }

    public DateTime? PaidOnDate { get; set; }
    public decimal Total { get; set; }
    public List<OrderProductDto> OrderProducts { get; set; }
}

