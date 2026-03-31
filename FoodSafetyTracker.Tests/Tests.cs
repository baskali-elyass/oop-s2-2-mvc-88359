using FoodSafetyTracker.Domain;
using FoodSafetyTracker.MVC.Controllers;
using FoodSafetyTracker.MVC.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FoodSafetyTracker.Tests;

file static class DbFactory
{
    public static ApplicationDbContext Create() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public static async Task<(ApplicationDbContext ctx, Premises premises)> WithPremisesAsync()
    {
        var ctx = Create();
        var p = new Premises { Name = "Test Place", Address = "1 St", Town = "Limerick", RiskRating = RiskRating.High };
        ctx.Premises.Add(p);
        await ctx.SaveChangesAsync();
        return (ctx, p);
    }

    public static async Task<(ApplicationDbContext ctx, Premises premises, Inspection inspection)> WithInspectionAsync()
    {
        var (ctx, p) = await WithPremisesAsync();
        var i = new Inspection { PremisesId = p.Id, InspectionDate = DateTime.Today.AddDays(-10), Score = 40, Outcome = InspectionOutcome.Fail };
        ctx.Inspections.Add(i);
        await ctx.SaveChangesAsync();
        return (ctx, p, i);
    }
}

public class PremisesDomainTests
{
    [Fact]
    public void Premises_DefaultInspections_IsEmptyCollection()
    {
        var p = new Premises();
        Assert.NotNull(p.Inspections);
        Assert.Empty(p.Inspections);
    }

    [Fact]
    public void Premises_Name_CanBeSet()
    {
        var p = new Premises { Name = "The Salt House", Address = "14 Quay St", Town = "Limerick", RiskRating = RiskRating.High };
        Assert.Equal("The Salt House", p.Name);
        Assert.Equal(RiskRating.High, p.RiskRating);
    }

    [Theory]
    [InlineData(RiskRating.Low)]
    [InlineData(RiskRating.Medium)]
    [InlineData(RiskRating.High)]
    public void Premises_AllRiskRatings_CanBeAssigned(RiskRating rating)
    {
        var p = new Premises { RiskRating = rating };
        Assert.Equal(rating, p.RiskRating);
    }
}

public class InspectionDomainTests
{
    [Fact]
    public void Inspection_DefaultFollowUps_IsEmptyCollection()
    {
        var i = new Inspection();
        Assert.NotNull(i.FollowUps);
        Assert.Empty(i.FollowUps);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Inspection_Score_CanBeSetInRange(int score)
    {
        var i = new Inspection { Score = score };
        Assert.Equal(score, i.Score);
    }

    [Theory]
    [InlineData(InspectionOutcome.Pass)]
    [InlineData(InspectionOutcome.Fail)]
    public void Inspection_Outcome_CanBeAssigned(InspectionOutcome outcome)
    {
        var i = new Inspection { Outcome = outcome };
        Assert.Equal(outcome, i.Outcome);
    }

    [Fact]
    public void Inspection_Notes_IsNullableAndCanBeSet()
    {
        var i = new Inspection { Notes = "Pest evidence." };
        Assert.Equal("Pest evidence.", i.Notes);
    }
}

public class FollowUpDomainTests
{
    [Fact]
    public void FollowUp_ClosedDate_IsNullByDefault()
    {
        var f = new FollowUp { DueDate = DateTime.Today, Status = FollowUpStatus.Open };
        Assert.Null(f.ClosedDate);
    }

    [Fact]
    public void FollowUp_Status_CanTransitionToClosed()
    {
        var f = new FollowUp { Status = FollowUpStatus.Open };
        f.Status = FollowUpStatus.Closed;
        f.ClosedDate = DateTime.Today;
        Assert.Equal(FollowUpStatus.Closed, f.Status);
        Assert.NotNull(f.ClosedDate);
    }

    [Fact]
    public void FollowUp_DueDateBeforeInspectionDate_IsLogicallyInvalid()
    {
        var inspectionDate = DateTime.Today;
        var dueDate = DateTime.Today.AddDays(-5);
        Assert.True(dueDate < inspectionDate);
    }
}

public class PremisesControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithAllPremises()
    {
        await using var ctx = DbFactory.Create();
        ctx.Premises.AddRange(
            new Premises { Name = "A", Address = "1 St", Town = "Limerick", RiskRating = RiskRating.Low },
            new Premises { Name = "B", Address = "2 St", Town = "Galway",   RiskRating = RiskRating.High }
        );
        await ctx.SaveChangesAsync();

        var controller = new PremisesController(ctx);
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Premises>>(view.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsView()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new PremisesController(ctx);

        var result = await controller.Details(premises.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Premises>(view.Model);
        Assert.Equal(premises.Id, model.Id);
    }

    [Fact]
    public async Task Details_WithNullId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithUnknownId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        await using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);
        var premises = new Premises { Name = "New Place", Address = "5 Rd", Town = "Cork", RiskRating = RiskRating.Medium };

        var result = await controller.Create(premises);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(1, await ctx.Premises.CountAsync());
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        await using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.Create(new Premises());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_WithValidId_ReturnsView()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new PremisesController(ctx);

        var result = await controller.Edit(premises.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Premises>(view.Model);
        Assert.Equal(premises.Id, model.Id);
    }

    [Fact]
    public async Task Edit_Get_WithNullId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);

        var result = await controller.Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new PremisesController(ctx);
        premises.Id = 999;

        var result = await controller.Edit(1, premises);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new PremisesController(ctx);
        premises.Name = "Updated Name";

        var result = await controller.Edit(premises.Id, premises);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Get_WithValidId_ReturnsView()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new PremisesController(ctx);

        var result = await controller.Delete(premises.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<Premises>(view.Model);
    }

    [Fact]
    public async Task Delete_Get_WithNullId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new PremisesController(ctx);

        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_WithNoInspections_DeletesAndRedirects()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new PremisesController(ctx);
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        var result = await controller.DeleteConfirmed(premises.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(0, await ctx.Premises.CountAsync());
    }

    [Fact]
    public async Task DeleteConfirmed_WithInspections_DoesNotDelete()
    {
        var (ctx, _, _) = await DbFactory.WithInspectionAsync();
        var premises = await ctx.Premises.FirstAsync();
        var controller = new PremisesController(ctx);
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        var result = await controller.DeleteConfirmed(premises.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(1, await ctx.Premises.CountAsync());
    }
}

public class InspectionsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithInspections()
    {
        var (ctx, _, _) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Inspection>>(view.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsView()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);

        var result = await controller.Details(inspection.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Inspection>(view.Model);
        Assert.Equal(inspection.Id, model.Id);
    }

    [Fact]
    public async Task Details_WithNullId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new InspectionsController(ctx);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithUnknownId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new InspectionsController(ctx);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var controller = new InspectionsController(ctx);
        var inspection = new Inspection
        {
            PremisesId = premises.Id,
            InspectionDate = DateTime.Today,
            Score = 75,
            Outcome = InspectionOutcome.Pass
        };

        var result = await controller.Create(inspection);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(1, await ctx.Inspections.CountAsync());
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        await using var ctx = DbFactory.Create();
        var controller = new InspectionsController(ctx);
        controller.ModelState.AddModelError("Score", "Required");

        var result = await controller.Create(new Inspection());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_WithNullId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new InspectionsController(ctx);

        var result = await controller.Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_WithUnknownId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new InspectionsController(ctx);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_WithValidId_ReturnsView()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);

        var result = await controller.Edit(inspection.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<Inspection>(view.Model);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);
        inspection.Id = 999;

        var result = await controller.Edit(1, inspection);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);
        inspection.Score = 90;

        var result = await controller.Edit(inspection.Id, inspection);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Get_WithValidId_ReturnsView()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);

        var result = await controller.Delete(inspection.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<Inspection>(view.Model);
    }

    [Fact]
    public async Task Delete_Get_WithNullId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new InspectionsController(ctx);

        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_WithValidId_DeletesAndRedirects()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new InspectionsController(ctx);

        var result = await controller.DeleteConfirmed(inspection.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(0, await ctx.Inspections.CountAsync());
    }
}

public class FollowUpsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithFollowUps()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        ctx.FollowUps.Add(new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(7), Status = FollowUpStatus.Open });
        await ctx.SaveChangesAsync();

        var controller = new FollowUpsController(ctx);
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<FollowUp>>(view.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new FollowUpsController(ctx);
        var followUp = new FollowUp
        {
            InspectionId = inspection.Id,
            DueDate = DateTime.Today.AddDays(14),
            Status = FollowUpStatus.Open
        };

        var result = await controller.Create(followUp);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(1, await ctx.FollowUps.CountAsync());
    }

    [Fact]
    public async Task Create_Post_DueDateBeforeInspection_ReturnsViewWithError()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var controller = new FollowUpsController(ctx);
        var followUp = new FollowUp
        {
            InspectionId = inspection.Id,
            DueDate = inspection.InspectionDate.AddDays(-5),
            Status = FollowUpStatus.Open
        };

        var result = await controller.Create(followUp);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("DueDate"));
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        var (ctx, _, _) = await DbFactory.WithInspectionAsync();
        var controller = new FollowUpsController(ctx);
        controller.ModelState.AddModelError("DueDate", "Required");

        var result = await controller.Create(new FollowUp());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Close_WithValidId_ClosesFollowUpAndRedirects()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var followUp = new FollowUp { InspectionId = inspection.Id, DueDate = DateTime.Today.AddDays(7), Status = FollowUpStatus.Open };
        ctx.FollowUps.Add(followUp);
        await ctx.SaveChangesAsync();

        var controller = new FollowUpsController(ctx);
        var result = await controller.Close(followUp.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var saved = await ctx.FollowUps.FindAsync(followUp.Id);
        Assert.Equal(FollowUpStatus.Closed, saved!.Status);
        Assert.NotNull(saved.ClosedDate);
    }

    [Fact]
    public async Task Close_WithUnknownId_ReturnsNotFound()
    {
        await using var ctx = DbFactory.Create();
        var controller = new FollowUpsController(ctx);

        var result = await controller.Close(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Close_AlreadyClosed_StatusRemainsUnchanged()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var closedDate = DateTime.Today.AddDays(-1);
        var followUp = new FollowUp
        {
            InspectionId = inspection.Id,
            DueDate = DateTime.Today.AddDays(7),
            Status = FollowUpStatus.Closed,
            ClosedDate = closedDate
        };
        ctx.FollowUps.Add(followUp);
        await ctx.SaveChangesAsync();

        var saved = await ctx.FollowUps.FindAsync(followUp.Id);
        Assert.Equal(FollowUpStatus.Closed, saved!.Status);
        Assert.Equal(closedDate, saved.ClosedDate);
    }
}

public class DashboardControllerTests
{
    [Fact]
    public async Task Index_NoFilter_ReturnsViewWithCorrectCounts()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();
        var today = DateTime.Today;

        ctx.Inspections.Add(new Inspection
        {
            PremisesId = inspection.PremisesId,
            InspectionDate = today,
            Score = 80,
            Outcome = InspectionOutcome.Pass
        });
        ctx.FollowUps.Add(new FollowUp
        {
            InspectionId = inspection.Id,
            DueDate = today.AddDays(-3),
            Status = FollowUpStatus.Open
        });
        await ctx.SaveChangesAsync();

        var controller = new DashboardController(ctx);
        var result = await controller.Index(null, null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(view.Model);
    }

    [Fact]
    public async Task Index_WithTownFilter_ReturnsFilteredCounts()
    {
        await using var ctx = DbFactory.Create();
        var p1 = new Premises { Name = "A", Address = "1 St", Town = "Limerick", RiskRating = RiskRating.High };
        var p2 = new Premises { Name = "B", Address = "2 St", Town = "Galway",   RiskRating = RiskRating.Low };
        ctx.Premises.AddRange(p1, p2);
        await ctx.SaveChangesAsync();

        var today = DateTime.Today;
        ctx.Inspections.AddRange(
            new Inspection { PremisesId = p1.Id, InspectionDate = today, Score = 50, Outcome = InspectionOutcome.Fail },
            new Inspection { PremisesId = p2.Id, InspectionDate = today, Score = 80, Outcome = InspectionOutcome.Pass }
        );
        await ctx.SaveChangesAsync();

        var controller = new DashboardController(ctx);
        var result = await controller.Index("Limerick", null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(view.Model);
    }

    [Fact]
    public async Task Index_WithRiskRatingFilter_ReturnsFilteredResults()
    {
        await using var ctx = DbFactory.Create();
        var p = new Premises { Name = "A", Address = "1 St", Town = "Cork", RiskRating = RiskRating.High };
        ctx.Premises.Add(p);
        await ctx.SaveChangesAsync();

        ctx.Inspections.Add(new Inspection { PremisesId = p.Id, InspectionDate = DateTime.Today, Score = 60, Outcome = InspectionOutcome.Pass });
        await ctx.SaveChangesAsync();

        var controller = new DashboardController(ctx);
        var result = await controller.Index(null, RiskRating.High);

        Assert.IsType<ViewResult>(result);
    }
}

public class BusinessLogicTests
{
    [Fact]
    public async Task OverdueFollowUps_ReturnsOnlyOpenAndPastDueDate()
    {
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();

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
        var (ctx, _, inspection) = await DbFactory.WithInspectionAsync();

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
        var (ctx, premises) = await DbFactory.WithPremisesAsync();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        ctx.Inspections.AddRange(
            new Inspection { PremisesId = premises.Id, InspectionDate = today,                    Score = 80, Outcome = InspectionOutcome.Pass },
            new Inspection { PremisesId = premises.Id, InspectionDate = today.AddDays(-2),        Score = 45, Outcome = InspectionOutcome.Fail },
            new Inspection { PremisesId = premises.Id, InspectionDate = monthStart.AddMonths(-1), Score = 70, Outcome = InspectionOutcome.Pass }
        );
        await ctx.SaveChangesAsync();

        var count = await ctx.Inspections.CountAsync(i => i.InspectionDate >= monthStart);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task Premises_WithInspections_CannotBeDeleted_ViaController()
    {
        var (ctx, _, _) = await DbFactory.WithInspectionAsync();
        var premises = await ctx.Premises.FirstAsync();
        var controller = new PremisesController(ctx);
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        await controller.DeleteConfirmed(premises.Id);

        Assert.Equal(1, await ctx.Premises.CountAsync());
    }
}
