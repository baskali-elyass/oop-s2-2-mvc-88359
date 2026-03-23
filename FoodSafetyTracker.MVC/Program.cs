using FoodSafetyTracker.MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "FoodSafetyTracker")
    .WriteTo.Console()
    .WriteTo.File("logs/tracker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting FoodSafetyTracker");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=foodsafety.db";

    builder.Services.AddDbContext<ApplicationDbContext>(o =>
        o.UseSqlite(connectionString));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services
        .AddDefaultIdentity<IdentityUser>(o => o.SignIn.RequireConfirmedAccount = false)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.Use(async (context, next) =>
    {
        var userName = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name ?? "anonymous"
            : "anonymous";
        Serilog.Context.LogContext.PushProperty("UserName", userName);
        await next();
    });

    if (app.Environment.IsDevelopment())
        app.UseMigrationsEndPoint();
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
    app.MapRazorPages();

    using (var scope = app.Services.CreateScope())
        await SeedData.InitialiseAsync(scope.ServiceProvider);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
