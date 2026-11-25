using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskEFC.Data;
using TaskEFC.Models;
using TaskEFC.ViewModels;

namespace TaskEFC.Controllers;

public class OrdersController : Controller
{
    private readonly ShoppingContext _context;

    public OrdersController(ShoppingContext context)
    {
        _context = context;
    }

    // GET: Orders or Orders/Index/5 (with the details of order 5)
    public async Task<IActionResult> Index(int? id)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Supermarket)
            .OrderBy(o => o.OrderDate)
            .ThenBy(o => o.Id)
            .ToListAsync();

        Order? selectedOrder = null;
        if (id != null)
        {
            selectedOrder = await FindWithDetailsAsync(id);
            if (selectedOrder == null)
            {
                return NotFound();
            }
        }

        return View(new OrderIndexViewModel { Orders = orders, SelectedOrder = selectedOrder });
    }

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        var order = await FindWithDetailsAsync(id);
        return order == null ? NotFound() : View(order);
    }

    // GET: Orders/Create
    public async Task<IActionResult> Create()
    {
        var model = new OrderCreateViewModel
        {
            OrderDate = DateTime.Today,
            Lines = await GetEmptyLinesAsync()
        };

        await PopulateSelectListsAsync();
        return View(model);
    }

    // POST: Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateViewModel model)
    {
        await ValidateReferencesAsync(model.CustomerId, model.SupermarketId);
        await ValidateLinesAsync(model);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(model.CustomerId, model.SupermarketId);
            return View(model);
        }

        var order = new Order
        {
            CustomerId = model.CustomerId!.Value,
            SupermarketId = model.SupermarketId!.Value,
            OrderDate = model.OrderDate!.Value,
            OrderDetails = model.Lines
                .Where(l => l.Quantity > 0)
                .Select(l => new OrderDetail { ProductId = l.ProductId, Quantity = l.Quantity!.Value })
                .ToList()
        };

        _context.Add(order);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = order.Id });
    }

    // GET: Orders/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var order = id == null
            ? null
            : await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        await PopulateSelectListsAsync(order.CustomerId, order.SupermarketId);
        return View(order);
    }

    // POST: Orders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,SupermarketId,OrderDate")] Order order)
    {
        if (id != order.Id)
        {
            return NotFound();
        }

        await ValidateReferencesAsync(order.CustomerId, order.SupermarketId);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(order.CustomerId, order.SupermarketId);
            return View(order);
        }

        try
        {
            _context.Update(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Orders.AnyAsync(o => o.Id == order.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index), new { id });
    }

    // GET: Orders/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var order = await FindWithDetailsAsync(id);
        return order == null ? NotFound() : View(order);
    }

    // POST: Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            // The order details are removed by the cascade delete configured in the context.
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Order?> FindWithDetailsAsync(int? id)
    {
        if (id == null)
        {
            return null;
        }

        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Supermarket)
            .Include(o => o.OrderDetails.OrderBy(d => d.Product!.Name))
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    private async Task PopulateSelectListsAsync(int? customerId = null, int? supermarketId = null)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
        var supermarkets = await _context.Supermarkets
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewData["Customers"] = new SelectList(customers, nameof(Customer.Id), nameof(Customer.FullName), customerId);
        ViewData["Supermarkets"] = new SelectList(supermarkets, nameof(Supermarket.Id), nameof(Supermarket.Name), supermarketId);
    }

    private Task<List<OrderLineViewModel>> GetEmptyLinesAsync() =>
        _context.Products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Select(p => new OrderLineViewModel { ProductId = p.Id, ProductName = p.Name, Price = p.Price })
            .ToListAsync();

    private async Task ValidateReferencesAsync(int? customerId, int? supermarketId)
    {
        if (customerId != null && !await _context.Customers.AnyAsync(c => c.Id == customerId))
        {
            ModelState.AddModelError(nameof(Order.CustomerId), "Select an existing customer.");
        }

        if (supermarketId != null && !await _context.Supermarkets.AnyAsync(s => s.Id == supermarketId))
        {
            ModelState.AddModelError(nameof(Order.SupermarketId), "Select an existing supermarket.");
        }
    }

    private async Task ValidateLinesAsync(OrderCreateViewModel model)
    {
        var products = await _context.Products.AsNoTracking().ToDictionaryAsync(p => p.Id);
        var ids = model.Lines.Select(l => l.ProductId).ToList();

        // A product was deleted while the form was open, or the request was altered.
        if (ids.Distinct().Count() != ids.Count || !ids.All(products.ContainsKey))
        {
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith(nameof(model.Lines))).ToList())
            {
                ModelState.Remove(key);
            }

            model.Lines = await GetEmptyLinesAsync();
            ModelState.AddModelError(string.Empty, "The product list has changed. Enter the quantities again.");
            return;
        }

        for (var i = 0; i < model.Lines.Count; i++)
        {
            var line = model.Lines[i];
            line.ProductName = products[line.ProductId].Name;
            line.Price = products[line.ProductId].Price;

            if (line.Quantity is > 0 and < OrderDetail.MinQuantity)
            {
                ModelState.AddModelError(
                    $"{nameof(model.Lines)}[{i}].{nameof(line.Quantity)}",
                    $"The minimum quantity is {OrderDetail.MinQuantity}.");
            }
        }

        if (!model.Lines.Any(l => l.Quantity > 0))
        {
            // A field with an invalid value already has its own message.
            var hasLineErrors = ModelState.Any(e =>
                e.Key.StartsWith(nameof(model.Lines)) && e.Value?.Errors.Count > 0);
            if (!hasLineErrors)
            {
                ModelState.AddModelError(string.Empty, "Specify the quantity of at least one product.");
            }
        }
    }
}
