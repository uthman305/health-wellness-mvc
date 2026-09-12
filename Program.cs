using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models;
using HealthWellnessMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Data layer: PostgreSQL via Npgsql, per the project's original design.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity replaces the manual bcrypt + JWT auth from the FastAPI version.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddScoped<BmiService>();
builder.Services.AddScoped<GamificationService>();
builder.Services.AddScoped<PredictiveAnalyticsService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed roles and default badges on startup.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    foreach (var role in new[] { "Student", "Lecturer", "Administrator" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    foreach (var (name, threshold) in GamificationService.DefaultBadges)
    {
        if (!db.Badges.Any(b => b.Name == name))
            db.Badges.Add(new Badge { Name = name, LogCountThreshold = threshold });
    }
    await db.SaveChangesAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
