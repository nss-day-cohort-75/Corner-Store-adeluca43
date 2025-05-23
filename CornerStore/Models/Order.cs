using System.ComponentModel.DataAnnotations;
namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int CashierId { get; set; }

    public Cashier? Cashier { get; set; } //navigation property for convenience links to cashier object( loads full cashier object with .include)

    public List<OrderProduct> OrderProducts { get; set; } = new(); // This list holds all the products included in the order
                                                                   // Each OrderProduct links this order to one product and tracks its quantity.
                                                                   // Used by EF Core to understand that one order can have multiple products.


    public decimal Total
    {
        get
        {
            return OrderProducts
                .Where(op => op.Product != null)
                .Sum(op => op.Product.Price * op.Quantity);
        }
    }
    // Calculates total by summing price * quantity for each product that exists (skips null products)


    public DateTime? PaidOnDate { get; set; } //datetime is nullable set only when payment is complete
}
