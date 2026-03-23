using FoodSafetyTracker.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.MVC.Data;

static class SeedData
{
    public static async Task InitialiseAsync(IServiceProvider services)
    {
        var ctx = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await ctx.Database.EnsureCreatedAsync();

        foreach (var role in new[] { "Admin", "Inspector", "Viewer" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        async Task CreateUser(string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) is null)
            {
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }

        await CreateUser("admin@fsi.ie", "Admin@1234", "Admin");
        await CreateUser("inspector@fsi.ie", "Inspector@1234", "Inspector");
        await CreateUser("viewer@fsi.ie", "Viewer@1234", "Viewer");

        if (await ctx.Premises.AnyAsync()) return;

        var premises = new List<Premises>
        {
            new() { Name = "The Salt House",       Address = "14 Quay St",      Town = "Limerick", RiskRating = RiskRating.High   },
            new() { Name = "Riverside Diner",      Address = "3 Shannon Rd",    Town = "Limerick", RiskRating = RiskRating.Medium },
            new() { Name = "King's Island Café",   Address = "7 Island Rd",     Town = "Limerick", RiskRating = RiskRating.Low    },
            new() { Name = "Treaty Burger Co",     Address = "22 O'Connell St", Town = "Limerick", RiskRating = RiskRating.Medium },
            new() { Name = "Claddagh Kitchen",     Address = "5 Dock Rd",       Town = "Galway",   RiskRating = RiskRating.High   },
            new() { Name = "Salthill Chipper",     Address = "1 Prom Walk",     Town = "Galway",   RiskRating = RiskRating.High   },
            new() { Name = "Eyre Square Sushi",    Address = "9 Eyre Sq",       Town = "Galway",   RiskRating = RiskRating.Low    },
            new() { Name = "West End Bistro",      Address = "40 Shop St",      Town = "Galway",   RiskRating = RiskRating.Medium },
            new() { Name = "Blackrock Smokehouse", Address = "2 Rockfield Ave", Town = "Waterford", RiskRating = RiskRating.High  },
            new() { Name = "Viking Quarter Grill", Address = "18 Colbeck St",   Town = "Waterford", RiskRating = RiskRating.Medium},
            new() { Name = "Suir Bakery",          Address = "6 Manor St",      Town = "Waterford", RiskRating = RiskRating.Low   },
            new() { Name = "The Granary Table",    Address = "11 Hanover St",   Town = "Waterford", RiskRating = RiskRating.Medium},
        };

        ctx.Premises.AddRange(premises);
        await ctx.SaveChangesAsync();

        var today = DateTime.Today;
        var inspections = new List<Inspection>
        {
            new() { PremisesId=premises[0].Id,  InspectionDate=today.AddDays(-3),   Score=42, Outcome=InspectionOutcome.Fail, Notes="Pest evidence near storage area." },
            new() { PremisesId=premises[0].Id,  InspectionDate=today.AddDays(-50),  Score=68, Outcome=InspectionOutcome.Pass, Notes="Previous issues resolved." },
            new() { PremisesId=premises[1].Id,  InspectionDate=today.AddDays(-7),   Score=85, Outcome=InspectionOutcome.Pass, Notes="Well maintained." },
            new() { PremisesId=premises[2].Id,  InspectionDate=today.AddDays(-12),  Score=91, Outcome=InspectionOutcome.Pass, Notes="Excellent hygiene standards." },
            new() { PremisesId=premises[3].Id,  InspectionDate=today.AddDays(-2),   Score=38, Outcome=InspectionOutcome.Fail, Notes="Temperature log not maintained." },
            new() { PremisesId=premises[4].Id,  InspectionDate=today.AddDays(-5),   Score=77, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[4].Id,  InspectionDate=today.AddDays(-65),  Score=55, Outcome=InspectionOutcome.Fail, Notes="Cross-contamination risk." },
            new() { PremisesId=premises[5].Id,  InspectionDate=today.AddDays(-1),   Score=48, Outcome=InspectionOutcome.Fail, Notes="Waste disposal non-compliant." },
            new() { PremisesId=premises[6].Id,  InspectionDate=today.AddDays(-9),   Score=93, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[7].Id,  InspectionDate=today.AddDays(-18),  Score=61, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[8].Id,  InspectionDate=today.AddDays(-4),   Score=44, Outcome=InspectionOutcome.Fail, Notes="Cold chain failure noted." },
            new() { PremisesId=premises[9].Id,  InspectionDate=today.AddDays(-11),  Score=79, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[10].Id, InspectionDate=today.AddDays(-22),  Score=88, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[11].Id, InspectionDate=today.AddDays(-6),   Score=53, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[0].Id,  InspectionDate=today.AddDays(-95),  Score=49, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[1].Id,  InspectionDate=today.AddDays(-110), Score=75, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[2].Id,  InspectionDate=today.AddDays(-48),  Score=82, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[3].Id,  InspectionDate=today.AddDays(-58),  Score=57, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[5].Id,  InspectionDate=today.AddDays(-8),   Score=70, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[6].Id,  InspectionDate=today.AddDays(-14),  Score=96, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[7].Id,  InspectionDate=today.AddDays(-20),  Score=46, Outcome=InspectionOutcome.Fail, Notes="Staff hygiene training overdue." },
            new() { PremisesId=premises[8].Id,  InspectionDate=today.AddDays(-33),  Score=72, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[9].Id,  InspectionDate=today.AddDays(-38),  Score=84, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[10].Id, InspectionDate=today.AddDays(-52),  Score=60, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[11].Id, InspectionDate=today.AddDays(-75),  Score=69, Outcome=InspectionOutcome.Pass },
        };

        ctx.Inspections.AddRange(inspections);
        await ctx.SaveChangesAsync();

        ctx.FollowUps.AddRange(
            new FollowUp { InspectionId = inspections[0].Id, DueDate = today.AddDays(10), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[0].Id, DueDate = today.AddDays(-6), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[4].Id, DueDate = today.AddDays(-8), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[6].Id, DueDate = today.AddDays(-12), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[7].Id, DueDate = today.AddDays(-4), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[10].Id, DueDate = today.AddDays(7), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[13].Id, DueDate = today.AddDays(5), Status = FollowUpStatus.Open },
            new FollowUp { InspectionId = inspections[4].Id, DueDate = today.AddDays(-25), Status = FollowUpStatus.Closed, ClosedDate = today.AddDays(-18) },
            new FollowUp { InspectionId = inspections[6].Id, DueDate = today.AddDays(-35), Status = FollowUpStatus.Closed, ClosedDate = today.AddDays(-28) },
            new FollowUp { InspectionId = inspections[10].Id, DueDate = today.AddDays(-20), Status = FollowUpStatus.Closed, ClosedDate = today.AddDays(-14) }
        );

        await ctx.SaveChangesAsync();
    }
}