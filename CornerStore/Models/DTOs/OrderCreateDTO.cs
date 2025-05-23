namespace CornerStore.Models.DTOs;

public class OrderCreateDto
{
    public int CashierId { get; set; }
    public List<OrderProductDto> Products { get; set; }
}
