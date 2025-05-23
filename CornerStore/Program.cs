using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

//add a cashier ( POST)
app.MapPost("/cashiers", (CornerStoreDbContext db, CashierDto newCashierDto) => //injecting DbContext to access and change database / CashierDto newCashierDto accepts data from client as object
{
    Cashier newCashier = new Cashier
    {
        FirstName = newCashierDto.FirstName,
        LastName = newCashierDto.LastName
    };

    db.Cashiers.Add(newCashier);
    db.SaveChanges();

    return Results.Created($"/cashiers/{newCashier.Id}", newCashierDto);
});


// Get a cashier with orders and order products( GET)
app.MapGet("/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    Cashier cashier = db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.Category)
        .FirstOrDefault(c => c.Id == id); //finds the first cashier with matching Id

    if (cashier == null)
    {
        return Results.NotFound();
    }

    List<OrderDto> orderDtos = new List<OrderDto>(); // Build list of full OrderDto objects

    foreach (Order order in cashier.Orders)
    {

        List<OrderProductDto> orderProductDtos = new List<OrderProductDto>(); // Build list of order product DTOs for each order

        foreach (OrderProduct op in order.OrderProducts)
        {
            if (op.Product != null && op.Product.Category != null)
            {
                OrderProductDto opDto = new OrderProductDto
                {
                    ProductId = op.ProductId,
                    Product = op.Product.ProductName,
                    Brand = op.Product.Brand,
                    Price = op.Product.Price,
                    Category = op.Product.Category.CategoryName,
                    Quantity = op.Quantity
                };

                orderProductDtos.Add(opDto);
            }
        }

        OrderDto orderDto = new OrderDto   //build the orderDto for this order
        {
            Id = order.Id,
            CashierId = order.CashierId.ToString(),
            PaidOnDate = order.PaidOnDate,
            Total = order.Total,
            OrderProducts = orderProductDtos
        };

        orderDtos.Add(orderDto);
    }

    CashierDto cashierDto = new CashierDto  // Build CashierDto with first name, last name, and  order 
    {
        FirstName = cashier.FirstName,
        LastName = cashier.LastName,
        Orders = orderDtos
    };
    return Results.Ok(cashierDto);
});



// get all products (GET)
app.MapGet("/products", (CornerStoreDbContext db, string? search) =>
{
    // Start with all products and include their category info
    IQueryable<Product> productsQuery = db.Products.Include(p => p.Category); // includes full category data for each product

    // If the search string is not null or empty skip it and return all
    if (!string.IsNullOrWhiteSpace(search))
    {
        // Convert search term to lowercase so we can compare case-insensitively
        search = search.ToLower();


        productsQuery = productsQuery.Where(p =>   // Filter the productsQuery:Only include products where:- the product name contains the search text OR the category name contains the search text
            p.ProductName.ToLower().Contains(search) ||
            p.Category.CategoryName.ToLower().Contains(search));
    }

    // Actually run the query and get the result as a list
    List<Product> products = productsQuery.ToList();

    List<ProductDto> productDtos = new List<ProductDto>(); //create a new list to hold the dtos

    foreach (Product product in products) //loop through each product and get it from the database
    {
        ProductDto dto = new ProductDto //create new productdto and fill the fields
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Brand = product.Brand,
            Price = product.Price,
            CategoryId = product.CategoryId,
            Category = product.Category.CategoryName
        };

        productDtos.Add(dto); //add the dto to the list
    }

    return Results.Ok(productDtos);
});



//add a product (POST)
app.MapPost("/products", (CornerStoreDbContext db, ProductDto newProductDto) => //accepting client input as productDto
{
    Product newProduct = new Product //creates new product object to be filled with incoming data
    {
        ProductName = newProductDto.ProductName,
        Brand = newProductDto.Brand,
        Price = newProductDto.Price,
        CategoryId = newProductDto.CategoryId
    };

    db.Products.Add(newProduct);
    db.SaveChanges();

    ProductDto createdDto = new ProductDto //create new productDto with all new saved values
    {
        Id = newProduct.Id,
        ProductName = newProduct.ProductName,
        Brand = newProduct.Brand,
        Price = newProduct.Price,
        CategoryId = newProduct.CategoryId,
        Category = db.Categories.Where(c => c.Id == newProduct.CategoryId).Select(c => c.CategoryName).FirstOrDefault() //(filtering through categories for matching ids to get name)
    };

    return Results.Created($"/products/{newProduct.Id}", createdDto);
});
/* test in swagger:
{
  "productName": "Gatorade",
  "brand": "PepsiCo",
  "price": 2.25,
  "categoryId": 2
}*/


//update a product (PUT for whole product update)
app.MapPut("/products/{id}", (CornerStoreDbContext db, int id, ProductDto updatedDto) => // ProductDto updatedDto comes from clients request body
{
    // Find the product by ID
    Product existingProduct = db.Products.FirstOrDefault(p => p.Id == id);

    if (existingProduct == null)
    {
        return Results.NotFound();
    }

    // Update the product's properties
    existingProduct.ProductName = updatedDto.ProductName;
    existingProduct.Brand = updatedDto.Brand;
    existingProduct.Price = updatedDto.Price;
    existingProduct.CategoryId = updatedDto.CategoryId;

    db.SaveChanges();

    // Build and return the updated DTO
    ProductDto responseProductDto = new ProductDto
    {
        Id = existingProduct.Id,
        ProductName = existingProduct.ProductName,
        Brand = existingProduct.Brand,
        Price = existingProduct.Price,
        CategoryId = existingProduct.CategoryId,
        Category = db.Categories
            .Where(c => c.Id == existingProduct.CategoryId).Select(c => c.CategoryName).FirstOrDefault() //filter to get category name
    };

    return Results.Ok(responseProductDto);
});
/* test with {
  "productName": "Gatorade Zero",
  "brand": "PepsiCo",
  "price": 2.50,
  "categoryId": 2
}
*/

//get an order details cashier,products, product categories 
app.MapGet("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    // Load the order and include all related data
    Order order = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .FirstOrDefault(o => o.Id == id);


    if (order == null)
    {
        return Results.NotFound();
    }

    // create list to hold product Dtos for this order
    List<OrderProductDto> orderProductDtos = new List<OrderProductDto>();

    foreach (OrderProduct op in order.OrderProducts) //loop through each orderProduct in the order
    {
        if (op.Product != null && op.Product.Category != null) //avoids error(ensures product and category arent null)
        {
            OrderProductDto dto = new OrderProductDto //create new dto with product info
            {
                ProductId = op.ProductId,
                Product = op.Product.ProductName,
                Brand = op.Product.Brand,
                Price = op.Product.Price,
                Category = op.Product.Category.CategoryName,
                Quantity = op.Quantity
            };

            orderProductDtos.Add(dto); //add product to dto list
        }
    }

    // Build the full order DTO
    OrderDto orderDto = new OrderDto
    {
        Id = order.Id,
        CashierId = order.CashierId.ToString(),
        CashierName = $"{order.Cashier.FirstName} {order.Cashier.LastName}",
        PaidOnDate = order.PaidOnDate,
        Total = order.Total,
        OrderProducts = orderProductDtos
    };

    // Return the full DTO including the cashiers name ( get from order.cashier)
    return Results.Ok(orderDto);
});



// get all orders query string param orderDate
app.MapGet("/orders", (CornerStoreDbContext db, DateTime? orderDate) =>
{
    // Start with all orders and include necessary related data
    IQueryable<Order> orderQuery = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category);

    // If orderDate is provided, filter by that date
    if (orderDate.HasValue)
    {
        orderQuery = orderQuery.Where(o => o.PaidOnDate.HasValue && o.PaidOnDate.Value.Date == orderDate.Value.Date); // Filter orders with PaidOnDate that matches the provided date (ignores time by using .Date)

    }

    List<Order> orders = orderQuery.ToList();  //executes the query and gets the list of orders

    // Create list to hold final dto
    List<OrderDto> results = new List<OrderDto>();

    foreach (Order order in orders)
    {
        // Build list of product DTOs for this order
        List<OrderProductDto> productDtos = new List<OrderProductDto>();

        foreach (OrderProduct op in order.OrderProducts)
        {
            if (op.Product != null && op.Product.Category != null)
            {
                OrderProductDto dto = new OrderProductDto
                {
                    ProductId = op.ProductId,
                    Product = op.Product.ProductName,
                    Brand = op.Product.Brand,
                    Price = op.Product.Price,
                    Category = op.Product.Category.CategoryName,
                    Quantity = op.Quantity
                };

                productDtos.Add(dto);
            }
        }

        OrderDto orderDto = new OrderDto //builds final dto with cashier and product info
        {
            Id = order.Id,
            CashierId = order.CashierId.ToString(),
            CashierName = $"{order.Cashier.FirstName} {order.Cashier.LastName}",
            PaidOnDate = order.PaidOnDate,
            Total = order.Total,
            OrderProducts = productDtos
        };
        results.Add(orderDto); //adds order to result list

    }
    return Results.Ok(results);
});


//delete an order by id 

app.MapDelete("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    // find the order in the database
    Order orderToDelete = db.Orders.FirstOrDefault(o => o.Id == id);

    if (orderToDelete == null)
    {
        return Results.NotFound();
    }

    // Remove the order
    db.Orders.Remove(orderToDelete);
    db.SaveChanges();
    return Results.NoContent();
});




// Create an order with products (POST)
app.MapPost("/orders", (CornerStoreDbContext db, OrderCreateDto dto) =>
{
    // Create the new Order
    Order newOrder = new Order
    {
        CashierId = dto.CashierId,
        PaidOnDate = DateTime.Now,
        OrderProducts = new List<OrderProduct>() // Initialize an empty list to hold associated products
    };

    // Loop through each product from the client
    foreach (OrderProductDto productDto in dto.Products)
    {
        OrderProduct orderProduct = new OrderProduct //creates a new orderproduct object to represent one product in the order
        {
            ProductId = productDto.ProductId,
            Quantity = productDto.Quantity
        };

        newOrder.OrderProducts.Add(orderProduct);
    }

    // Save to database
    db.Orders.Add(newOrder);
    db.SaveChanges();

    return Results.Created($"/orders/{newOrder.Id}", new { orderId = newOrder.Id });
});
/* test with {
  "cashierId": 1,
  "products": [
    { "productId": 2, "quantity": 1 },
    { "productId": 5, "quantity": 2 }
  ]
*/


app.Run();

//don't move or change this!
public partial class Program { }