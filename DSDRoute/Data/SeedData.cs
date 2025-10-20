using DSDRoute.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DSDRoute.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminRole = "Admin";
            string userRole = "User";
            string adminEmail = "admin@domain.com";
            string adminPassword = "Admin@123";

            // Create roles
            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!await roleManager.RoleExistsAsync(userRole))
                await roleManager.CreateAsync(new IdentityRole(userRole));

            // Create admin user
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }

            // Create sample sales rep
            var salesRepEmail = "salesrep@domain.com";
            var salesRep = await userManager.FindByEmailAsync(salesRepEmail);
            if (salesRep == null)
            {
                salesRep = new ApplicationUser
                {
                    UserName = salesRepEmail,
                    Email = salesRepEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(salesRep, "SalesRep@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(salesRep, userRole);
                }
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Name = "Coca Cola 330ml", SKU = "CC-330", Price = 1.50m, StockQty = 1000, Category = "Beverages" },
                    new Product { Name = "Pepsi 330ml", SKU = "PP-330", Price = 1.45m, StockQty = 800, Category = "Beverages" },
                    new Product { Name = "Sprite 330ml", SKU = "SP-330", Price = 1.40m, StockQty = 600, Category = "Beverages" },
                    new Product { Name = "Fanta Orange 330ml", SKU = "FO-330", Price = 1.40m, StockQty = 500, Category = "Beverages" },
                    new Product { Name = "Red Bull 250ml", SKU = "RB-250", Price = 2.50m, StockQty = 200, Category = "Energy Drinks" },
                    new Product { Name = "Monster Energy 500ml", SKU = "ME-500", Price = 3.00m, StockQty = 150, Category = "Energy Drinks" },
                    new Product { Name = "Doritos Nacho Cheese", SKU = "DN-NC", Price = 2.25m, StockQty = 300, Category = "Snacks" },
                    new Product { Name = "Lays Classic 150g", SKU = "LC-150", Price = 1.75m, StockQty = 400, Category = "Snacks" },
                    new Product { Name = "Kit Kat 4-Finger", SKU = "KK-4F", Price = 1.25m, StockQty = 250, Category = "Chocolate" },
                    new Product { Name = "Snickers Bar", SKU = "SN-BAR", Price = 1.30m, StockQty = 220, Category = "Chocolate" }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            // Seed Shops
            if (!context.Shops.Any())
            {
                var shops = new List<Shop>
                {
                    new Shop { Name = "Corner Store Market", Location = "123 Main St", Contact = "+1-555-0101", Address = "123 Main St, Downtown", Email = "corner@store.com", CreatedBy = adminUser.Id },
                    new Shop { Name = "Quick Stop Convenience", Location = "456 Oak Ave", Contact = "+1-555-0102", Address = "456 Oak Ave, Uptown", Email = "quick@stop.com", CreatedBy = adminUser.Id },
                    new Shop { Name = "Fresh Mart", Location = "789 Pine Rd", Contact = "+1-555-0103", Address = "789 Pine Rd, Midtown", Email = "fresh@mart.com", CreatedBy = adminUser.Id },
                    new Shop { Name = "City Grocery", Location = "321 Elm St", Contact = "+1-555-0104", Address = "321 Elm St, City Center", Email = "city@grocery.com", CreatedBy = adminUser.Id },
                    new Shop { Name = "Neighborhood Market", Location = "654 Maple Dr", Contact = "+1-555-0105", Address = "654 Maple Dr, Suburbs", Email = "neighbor@market.com", CreatedBy = adminUser.Id }
                };

                context.Shops.AddRange(shops);
                await context.SaveChangesAsync();
            }

            // Seed sample orders for demonstration
            if (!context.Orders.Any() && salesRep != null)
            {
                var shop = context.Shops.First();
                var products = context.Products.Take(3).ToList();

                var order = new Order
                {
                    ShopId = shop.Id,
                    SalesRepId = salesRep.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    Status = OrderStatus.Pending,
                    Notes = "Sample order for testing"
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                var orderItems = new List<OrderItem>
                {
                    new OrderItem { OrderId = order.Id, ProductId = products[0].Id, Quantity = 10, Price = products[0].Price },
                    new OrderItem { OrderId = order.Id, ProductId = products[1].Id, Quantity = 5, Price = products[1].Price },
                    new OrderItem { OrderId = order.Id, ProductId = products[2].Id, Quantity = 8, Price = products[2].Price }
                };

                context.OrderItems.AddRange(orderItems);
                order.TotalAmount = orderItems.Sum(oi => oi.Total);
                await context.SaveChangesAsync();
            }
        }
    }
}