using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEFC.Data;
using TaskEFC.Models;

namespace TaskEFC.Controllers;

public class ProductsController : Controller
{
    private readonly ShoppingContext _context;

    public ProductsController(ShoppingContext context)
    {
        _context = context;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        return View(await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync());
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        var product = await FindAsync(id);
        return product == null ? NotFound() : View(product);
    }

    // GET: Products/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Price")] Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        _context.Add(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var product = await FindAsync(id);
        return product == null ? NotFound() : View(product);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price")] Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        try
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Products.AnyAsync(p => p.Id == product.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var product = await FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (await IsOrderedAsync(product.Id))
        {
            ViewData["DeleteError"] = "This product is used in orders and cannot be deleted. Remove it from the orders first.";
        }

        return View(product);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await IsOrderedAsync(id))
        {
            return RedirectToAction(nameof(Delete), new { id });
        }

        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Product?> FindAsync(int? id) =>
        id == null ? null : await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    private Task<bool> IsOrderedAsync(int productId) =>
        _context.OrderDetails.AnyAsync(d => d.ProductId == productId);
}
