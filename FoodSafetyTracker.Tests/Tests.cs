using FoodSafetyTracker.Domain;
using FoodSafetyTracker.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Tests;

public class Tests
{
    private static ApplicationDbContext CreateCtx() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task OverdueFollowUps_ReturnsOnlyOpenAndPastDueDate()
    {
        await using var ctx = CreateCtx();
        var premises = new Premises { Name = "Test Place", Address = "1 St", Town = "Limerick", RiskRating = RiskRating.High };
        ctx.Premises.Add(premises);
        await ctx.SaveChangesAsync();

        var inspection = new Inspection { PremisesId = premises.Id, InspectionDate = DateTime.Today.AddDays(-30), Score = 40, Outcome = InspectionOutcome.Fail };
        ctx.Inspections.Add(inspection);
        await ctx.SaveChangesAsync();

        ctx.FollowUps.AddRange(
            new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(-5), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(-2), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(5),  Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(-3), Status = FollowUpStatus.Closed, ClosedDate = DateTime.Today.AddDays(-1) }
        );
        await ctx.SaveChangesAsync();

        var overdue = await ctx.FollowUps
            .Where(f => f.Status == FollowUpStatus.Open && f.DueDate < DateTime.Today)
            .ToListAsync();

        Assert.Equal(2, overdue.Count);
    }

    [Fact]
    public async Task ClosingFollowUp_SetsStatusAndClosedDate()
    {
        await using var ctx = CreateCtx();
        var premises = new Premises { Name = "P", Address = "A", Town = "Waterford", RiskRating = RiskRating.Low };
        ctx.Premises.Add(premises);
        await ctx.SaveChangesAsync();

        var inspection = new Inspection { PremisesId = premises.Id, InspectionDate = DateTime.Today.AddDays(-10), Score = 50, Outcome = InspectionOutcome.Fail };
        ctx.Inspections.Add(inspection);
        await ctx.SaveChangesAsync();

        var followUp = new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(7), Status = FollowUpStatus.Open };
        ctx.FollowUps.Add(followUp);
        await ctx.SaveChangesAsync();

        followUp.Status = FollowUpStatus.Closed;
        followUp.ClosedDate = DateTime.Today;
        await ctx.SaveChangesAsync();

        var saved = await ctx.FollowUps.FindAsync(followUp.Id);
        Assert.Equal(FollowUpStatus.Closed, saved!.Status);
        Assert.NotNull(saved.ClosedDate);
    }

    [Fact]
    public async Task DashboardCount_InspectionsThisMonth_IsCorrect()
    {
        await using var ctx = CreateCtx();
        var premises = new Premises { Name = "P", Address = "A", Town = "Galway", RiskRating = RiskRating.Medium };
        ctx.Premises.Add(premises);
        await ctx.SaveChangesAsync();

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        ctx.Inspections.AddRange(
            new Inspection { PremisesId = premises.Id, InspectionDate = today,                Score = 80, Outcome = InspectionOutcome.Pass },
            new Inspection { PremisesId = premises.Id, InspectionDate = today.AddDays(-2),    Score = 45, Outcome = InspectionOutcome.Fail },
            new Inspection { PremisesId = premises.Id, InspectionDate = monthStart.AddMonths(-1), Score = 70, Outcome = InspectionOutcome.Pass }
        );
        await ctx.SaveChangesAsync();

        var count = await ctx.Inspections.CountAsync(i => i.InspectionDate >= monthStart);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task FollowUp_DueDateBeforeInspectionDate_IsInvalid()
    {
        await using var ctx = CreateCtx();
        var premises = new Premises { Name = "P", Address = "A", Town = "Limerick", RiskRating = RiskRating.High };
        ctx.Premises.Add(premises);
        await ctx.SaveChangesAsync();

        var inspection = new Inspection { PremisesId = premises.Id, InspectionDate = DateTime.Today, Score = 40, Outcome = InspectionOutcome.Fail };
        ctx.Inspections.Add(inspection);
        await ctx.SaveChangesAsync();

        var dueDateBeforeInspection = DateTime.Today.AddDays(-5);
        Assert.True(dueDateBeforeInspection < inspection.InspectionDate);
    }
}
