using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskEFC.Data;
using TaskEFC.Models;

namespace TaskEFC.Controllers;

/// <summary>
/// Manages the product lines of an order. They are displayed on the Orders page.
/// </summary>
public class OrderDetailsController : Controller
{
    private readonly ShoppingContext _context;

    public OrderDetailsController(ShoppingContext context)
    {
        _context = context;
    }

    // GET: OrderDetails/Create?orderId=5
    public async Task<IActionResult> Create(int? orderId)
    {
        if (orderId == null || !await _context.Orders.AnyAsync(o => o.Id == orderId))
        {
            return NotFound();
        }

        await PopulateProductsAsync();
        return View(new OrderDetail { OrderId = orderId.Value, Quantity = 1 });
    }

    // POST: OrderDetails/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("OrderId,ProductId,Quantity")] OrderDetail orderDetail)
    {
        if (!await _context.Orders.AnyAsync(o => o.Id == orderDetail.OrderId))
        {
            return NotFound();
        }

        await ValidateProductAsync(orderDetail.OrderId, orderDetail.ProductId);

        if (!ModelState.IsValid)
        {
            await PopulateProductsAsync(orderDetail.ProductId);
            return View(orderDetail);
        }

        _context.Add(orderDetail);
        await _context.SaveChangesAsync();
        return RedirectToOrder(orderDetail.OrderId);
    }

    // GET: OrderDetails/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var orderDetail = await FindWithProductAsync(id);
        if (orderDetail == null)
        {
            return NotFound();
        }

        await PopulateProductsAsync(orderDetail.ProductId);
        return View(orderDetail);
    }

    // POST: OrderDetails/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Quantity")] OrderDetail input)
    {
        if (id != input.Id)
        {
            return NotFound();
        }

        // Only the product and quantity are editable; the line always stays in its order.
        var orderDetail = await _context.OrderDetails.FindAsync(id);
        if (orderDetail == null)
        {
            return NotFound();
        }

        await ValidateProductAsync(orderDetail.OrderId, input.ProductId, id);

        if (!ModelState.IsValid)
        {
            input.OrderId = orderDetail.OrderId;
            await PopulateProductsAsync(input.ProductId);
            return View(input);
        }

        orderDetail.ProductId = input.ProductId;
        orderDetail.Quantity = input.Quantity;
        await _context.SaveChangesAsync();
        return RedirectToOrder(orderDetail.OrderId);
    }

    // GET: OrderDetails/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var orderDetail = await FindWithProductAsync(id);
        return orderDetail == null ? NotFound() : View(orderDetail);
    }

    // POST: OrderDetails/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var orderDetail = await _context.OrderDetails.FindAsync(id);
        if (orderDetail == null)
        {
            return RedirectToAction(nameof(OrdersController.Index), "Orders");
        }

        _context.OrderDetails.Remove(orderDetail);
        await _context.SaveChangesAsync();
        return RedirectToOrder(orderDetail.OrderId);
    }

    private IActionResult RedirectToOrder(int orderId) =>
        RedirectToAction(nameof(OrdersController.Index), "Orders", new { id = orderId });

    private async Task<OrderDetail?> FindWithProductAsync(int? id) =>
        id == null
            ? null
            : await _context.OrderDetails
                .AsNoTracking()
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

    private async Task PopulateProductsAsync(int? productId = null)
    {
        var products = await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();

        // The price is shown next to the name so the user sees it while choosing.
        var items = products.Select(p => new { p.Id, Text = $"{p.Name} ({p.Price:F2})" });

        ViewData["Products"] = new SelectList(items, "Id", "Text", productId);
    }

    private async Task ValidateProductAsync(int orderId, int productId, int? currentDetailId = null)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == productId))
        {
            ModelState.AddModelError(nameof(OrderDetail.ProductId), "Select an existing product.");
        }
        else if (await _context.OrderDetails.AnyAsync(d =>
                     d.OrderId == orderId && d.ProductId == productId && d.Id != currentDetailId))
        {
            ModelState.AddModelError(nameof(OrderDetail.ProductId),
                "This product is already in the order. Edit its quantity instead.");
        }
    }
}
