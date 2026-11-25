using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEFC.Data;
using TaskEFC.Models;
using TaskEFC.ViewModels;

namespace TaskEFC.Controllers;

public class SupermarketsController : Controller
{
    private const int PageSize = 3;

    private readonly ShoppingContext _context;

    public SupermarketsController(ShoppingContext context)
    {
        _context = context;
    }

    // GET: Supermarkets?pageNumber=2
    public async Task<IActionResult> Index(int pageNumber = 1)
    {
        var supermarkets = _context.Supermarkets.AsNoTracking().OrderBy(s => s.Id);

        return View(await PaginatedList<Supermarket>.CreateAsync(supermarkets, pageNumber, PageSize));
    }

    // GET: Supermarkets/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        var supermarket = await FindAsync(id);
        return supermarket == null ? NotFound() : View(supermarket);
    }

    // GET: Supermarkets/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Supermarkets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Address")] Supermarket supermarket)
    {
        if (!ModelState.IsValid)
        {
            return View(supermarket);
        }

        _context.Add(supermarket);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Supermarkets/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var supermarket = await FindAsync(id);
        return supermarket == null ? NotFound() : View(supermarket);
    }

    // POST: Supermarkets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Supermarket supermarket)
    {
        if (id != supermarket.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(supermarket);
        }

        try
        {
            _context.Update(supermarket);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Supermarkets.AnyAsync(s => s.Id == supermarket.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Supermarkets/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var supermarket = await FindAsync(id);
        if (supermarket == null)
        {
            return NotFound();
        }

        if (await HasOrdersAsync(supermarket.Id))
        {
            ViewData["DeleteError"] = "This supermarket has orders and cannot be deleted. Delete the orders first.";
        }

        return View(supermarket);
    }

    // POST: Supermarkets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await HasOrdersAsync(id))
        {
            return RedirectToAction(nameof(Delete), new { id });
        }

        var supermarket = await _context.Supermarkets.FindAsync(id);
        if (supermarket != null)
        {
            _context.Supermarkets.Remove(supermarket);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Supermarket?> FindAsync(int? id) =>
        id == null ? null : await _context.Supermarkets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    private Task<bool> HasOrdersAsync(int supermarketId) =>
        _context.Orders.AnyAsync(o => o.SupermarketId == supermarketId);
}
