using FoodSafetyTracker.Domain;
using FoodSafetyTracker.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FoodSafetyTracker.MVC.Controllers;

[Authorize]
public class InspectionsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
        => View(await context.Inspections.Include(i => i.Premises)
            .OrderByDescending(i => i.InspectionDate).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var i = await context.Inspections.Include(i => i.Premises).Include(i => i.FollowUps)
            .FirstOrDefaultAsync(i => i.Id == id);
        return i is null ? NotFound() : View(i);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public IActionResult Create()
    {
        ViewData["PremisesId"] = new SelectList(context.Premises.OrderBy(p => p.Name), "Id", "Name");
        return View(new Inspection { InspectionDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create([Bind("PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
    {
        if (!ModelState.IsValid)
        {
            ViewData["PremisesId"] = new SelectList(context.Premises.OrderBy(p => p.Name), "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }
        context.Add(inspection);
        await context.SaveChangesAsync();
        Log.Information("Inspection created: {Id} for PremisesId {PremisesId}, Outcome: {Outcome}, Score: {Score}",
            inspection.Id, inspection.PremisesId, inspection.Outcome, inspection.Score);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var i = await context.Inspections.FindAsync(id);
        if (i is null) return NotFound();
        ViewData["PremisesId"] = new SelectList(context.Premises.OrderBy(p => p.Name), "Id", "Name", i.PremisesId);
        return View(i);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
    {
        if (id != inspection.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewData["PremisesId"] = new SelectList(context.Premises.OrderBy(p => p.Name), "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }
        try
        {
            context.Update(inspection);
            await context.SaveChangesAsync();
            Log.Information("Inspection updated: {Id}", inspection.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Log.Error(ex, "Concurrency error updating Inspection {Id}", id);
            if (!context.Inspections.Any(i => i.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var i = await context.Inspections.Include(i => i.Premises).FirstOrDefaultAsync(i => i.Id == id);
        return i is null ? NotFound() : View(i);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var i = await context.Inspections.FindAsync(id);
        if (i is not null)
        {
            context.Inspections.Remove(i);
            await context.SaveChangesAsync();
            Log.Information("Inspection deleted: {Id}", id);
        }
        return RedirectToAction(nameof(Index));
    }
}
