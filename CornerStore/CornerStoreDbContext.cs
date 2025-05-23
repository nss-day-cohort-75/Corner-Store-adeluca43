using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context) //This is the constructor. It tells EF Core to pass in the options like connection strings and settings when it creates the context
    {

    }
    // DbSets for each table ( each one represents a table in the database)
    public DbSet<Cashier> Cashiers => Set<Cashier>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite key for OrderProduct means each combo of OrderId and ProductId is unique
        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        // Seed Cashiers
        modelBuilder.Entity<Cashier>().HasData(
            new Cashier { Id = 1, FirstName = "John", LastName = "Jones" },
            new Cashier { Id = 2, FirstName = "Taylor", LastName = "Smith" },
            new Cashier { Id = 3, FirstName = "Jordan", LastName = "Lee" },
        new Cashier { Id = 4, FirstName = "Chris", LastName = "Nguyen" },
        new Cashier { Id = 5, FirstName = "Morgan", LastName = "Taylor" }
        );

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "Snacks" },
            new Category { Id = 2, CategoryName = "Drinks" },
            new Category { Id = 3, CategoryName = "Household" },
        new Category { Id = 4, CategoryName = "Candy" },
        new Category { Id = 5, CategoryName = "Frozen Foods" }
        );

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Chips", Brand = "Lays", Price = 2.50m, CategoryId = 1 },
            new Product { Id = 2, ProductName = "Cola", Brand = "Coca-Cola", Price = 1.75m, CategoryId = 2 },
            new Product { Id = 3, ProductName = "Dish Soap", Brand = "Dawn", Price = 3.00m, CategoryId = 3 },
        new Product { Id = 4, ProductName = "Chocolate Bar", Brand = "Hershey", Price = 1.25m, CategoryId = 4 },
        new Product { Id = 5, ProductName = "Frozen Pizza", Brand = "DiGiorno", Price = 6.99m, CategoryId = 5 }
        );

        // Seed Orders
        modelBuilder.Entity<Order>().HasData(
             new Order { Id = 1, CashierId = 1, PaidOnDate = new DateTime(2025, 5, 20, 10, 0, 0) },
        new Order { Id = 2, CashierId = 2, PaidOnDate = new DateTime(2025, 5, 20, 11, 30, 0) },
        new Order { Id = 3, CashierId = 3, PaidOnDate = new DateTime(2025, 5, 21, 9, 15, 0) },
        new Order { Id = 4, CashierId = 4, PaidOnDate = null },
        new Order { Id = 5, CashierId = 5, PaidOnDate = new DateTime(2025, 5, 21, 14, 45, 0) }
        );

        // Seed OrderProducts
        modelBuilder.Entity<OrderProduct>().HasData(
            new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
        new OrderProduct { OrderId = 1, ProductId = 2, Quantity = 1 },
        new OrderProduct { OrderId = 2, ProductId = 4, Quantity = 3 },
        new OrderProduct { OrderId = 3, ProductId = 3, Quantity = 1 },
        new OrderProduct { OrderId = 4, ProductId = 5, Quantity = 2 },
        new OrderProduct { OrderId = 5, ProductId = 1, Quantity = 1 },
        new OrderProduct { OrderId = 5, ProductId = 2, Quantity = 1 }
        );
    }
}