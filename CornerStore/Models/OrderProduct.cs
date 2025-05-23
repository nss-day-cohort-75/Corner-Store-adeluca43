using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class OrderProduct
{
    [Required]
    public int ProductId { get; set; }

    public Product? Product { get; set; } // nullable naviagation property for convenience ( allows .include to work)

    [Required]
    public int OrderId { get; set; }

    public Order? Order { get; set; }

    [Required]
    public int Quantity { get; set; }
}

