using Microsoft.EntityFrameworkCore;
using TaskEFC.Models;

namespace TaskEFC.Data;

public static class SampleData
{
    /// <summary>
    /// Applies pending migrations and fills an empty database with demo data.
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ShoppingContext>();

        context.Database.Migrate();

        if (context.Customers.Any() || context.Products.Any() || context.Supermarkets.Any())
        {
            return;
        }

        var shura = new Customer { FirstName = "Shura", LastName = "Balaganov", Address = "Odessa", Discount = Discount.R };
        var ostap = new Customer { FirstName = "Ostap", LastName = "Bender", Address = "Rio de Zhmerinka", Discount = Discount.O };

        context.Customers.AddRange(
            shura,
            ostap,
            new Customer { FirstName = "Madam", LastName = "Gritsatsuieva", Address = "Ensk", Discount = Discount.O },
            new Customer { FirstName = "Adam", LastName = "Kozlevych", Address = "Chernomorsk", Discount = Discount.O },
            new Customer { FirstName = "Semiuel M.", LastName = "Panikovskyi", Address = "Chernomorsk", Discount = Discount.V },
            new Customer { FirstName = "Zosya", LastName = "Sinitsina", Address = "Chernomorsk", Discount = Discount.O });

        var billa = new Supermarket { Name = "Billa", Address = "Odessa" };
        var bigPocket = new Supermarket { Name = "Big Pocket", Address = "Lviv" };

        context.Supermarkets.AddRange(
            billa,
            bigPocket,
            new Supermarket { Name = "Wellmart", Address = "Chernivtsi, Rus'ka" },
            new Supermarket { Name = "Silpo", Address = "Kyiv, Khreshchatyk" },
            new Supermarket { Name = "ATB", Address = "Dnipro" },
            new Supermarket { Name = "Auchan", Address = "Kharkiv" },
            new Supermarket { Name = "Novus", Address = "Rivne" });

        var banana = new Product { Name = "Banana", Price = 45.50 };
        var butter = new Product { Name = "Butter", Price = 89.90 };
        var cola = new Product { Name = "Cola", Price = 32.00 };
        var bread = new Product { Name = "Bread", Price = 24.75 };
        var milk = new Product { Name = "Milk", Price = 41.20 };

        context.Products.AddRange(banana, butter, cola, bread, milk);

        context.Orders.AddRange(
            new Order
            {
                Customer = shura,
                Supermarket = billa,
                OrderDate = new DateTime(2020, 8, 23),
                OrderDetails =
                {
                    new OrderDetail { Product = banana, Quantity = 1 },
                    new OrderDetail { Product = butter, Quantity = 2 },
                    new OrderDetail { Product = cola, Quantity = 1 }
                }
            },
            new Order
            {
                Customer = ostap,
                Supermarket = bigPocket,
                OrderDate = new DateTime(2020, 9, 2),
                OrderDetails =
                {
                    new OrderDetail { Product = bread, Quantity = 2 },
                    new OrderDetail { Product = milk, Quantity = 1.5 }
                }
            });

        context.SaveChanges();
    }
}
