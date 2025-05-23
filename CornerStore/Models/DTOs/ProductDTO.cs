namespace CornerStore.Models.DTOs;

public class ProductDto
{
    public int CategoryId { get; set; }
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
}