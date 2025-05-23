using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Cashier
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}"; // Read-only computed property — combines first and last name for convenience


    public List<Order> Orders { get; set; } = new();   // A cashier can be linked to many orders

}