
namespace CornerStore.Models.DTOs;

public class OrderProductDto
{
    public int ProductId { get; set; }
    public string Product { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int Quantity { get; set; }
}
