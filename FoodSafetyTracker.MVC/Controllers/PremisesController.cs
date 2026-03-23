using FoodSafetyTracker.Domain;
using FoodSafetyTracker.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FoodSafetyTracker.MVC.Controllers;

[Authorize]
public class PremisesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
        => View(await context.Premises.OrderBy(p => p.Town).ThenBy(p => p.Name).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var p = await context.Premises.Include(p => p.Inspections).FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : View(p);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create([Bind("Name,Address,Town,RiskRating")] Premises premises)
    {
        if (!ModelState.IsValid) return View(premises);
        context.Add(premises);
        await context.SaveChangesAsync();
        Log.Information("Premises created: {Id} {Name} in {Town}", premises.Id, premises.Name, premises.Town);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var p = await context.Premises.FindAsync(id);
        return p is null ? NotFound() : View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
    {
        if (id != premises.Id) return NotFound();
        if (!ModelState.IsValid) return View(premises);
        try
        {
            context.Update(premises);
            await context.SaveChangesAsync();
            Log.Information("Premises updated: {Id} {Name}", premises.Id, premises.Name);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Log.Error(ex, "Concurrency error updating Premises {Id}", id);
            if (!context.Premises.Any(p => p.Id == premises.Id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var p = await context.Premises.FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : View(p);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await context.Premises.Include(p => p.Inspections).FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return NotFound();
        if (p.Inspections.Any())
        {
            TempData["Error"] = "Cannot delete a premises that has inspections.";
            return RedirectToAction(nameof(Index));
        }
        context.Premises.Remove(p);
        await context.SaveChangesAsync();
        Log.Information("Premises deleted: {Id}", id);
        return RedirectToAction(nameof(Index));
    }
}
