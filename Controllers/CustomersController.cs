using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEFC.Data;
using TaskEFC.Models;
using TaskEFC.ViewModels;

namespace TaskEFC.Controllers;

public class CustomersController : Controller
{
    private readonly ShoppingContext _context;

    public CustomersController(ShoppingContext context)
    {
        _context = context;
    }

    // GET: Customers?sortOrder=name_desc&searchString=B
    public async Task<IActionResult> Index(string? sortOrder, string? searchString)
    {
        searchString = searchString?.Trim();

        IQueryable<Customer> customers = _context.Customers.AsNoTracking();

        if (!string.IsNullOrEmpty(searchString))
        {
            customers = customers.Where(c =>
                c.LastName.Contains(searchString) || c.FirstName.Contains(searchString));
        }

        // Id is the last sort key so that equal values keep a stable order.
        var sorted = sortOrder switch
        {
            CustomerSortOrder.LastNameDescending => customers.OrderByDescending(c => c.LastName)
                .ThenByDescending(c => c.FirstName),
            CustomerSortOrder.AddressAscending => customers.OrderBy(c => c.Address)
                .ThenBy(c => c.LastName),
            CustomerSortOrder.AddressDescending => customers.OrderByDescending(c => c.Address)
                .ThenBy(c => c.LastName),
            _ => customers.OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
        };

        return View(new CustomerIndexViewModel
        {
            Customers = await sorted.ThenBy(c => c.Id).ToListAsync(),
            SortOrder = sortOrder,
            SearchString = searchString
        });
    }

    // GET: Customers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        var customer = await FindAsync(id);
        return customer == null ? NotFound() : View(customer);
    }

    // GET: Customers/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LastName,FirstName,Address,Discount")] Customer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        _context.Add(customer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var customer = await FindAsync(id);
        return customer == null ? NotFound() : View(customer);
    }

    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,Address,Discount")] Customer customer)
    {
        if (id != customer.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        try
        {
            _context.Update(customer);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Customers.AnyAsync(c => c.Id == customer.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var customer = await FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        if (await HasOrdersAsync(customer.Id))
        {
            ViewData["DeleteError"] = "This customer has orders and cannot be deleted. Delete the orders first.";
        }

        return View(customer);
    }

    // POST: Customers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await HasOrdersAsync(id))
        {
            return RedirectToAction(nameof(Delete), new { id });
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Customer?> FindAsync(int? id) =>
        id == null ? null : await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    private Task<bool> HasOrdersAsync(int customerId) =>
        _context.Orders.AnyAsync(o => o.CustomerId == customerId);
}
