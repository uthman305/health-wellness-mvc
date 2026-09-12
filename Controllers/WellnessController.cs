using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models;
using HealthWellnessMVC.Models.ViewModels;
using HealthWellnessMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Controllers;

// Wellness Logging and BMI Module + personalised dashboard.
[Authorize]
public class WellnessController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly BmiService _bmiService;
    private readonly GamificationService _gamification;
    private readonly PredictiveAnalyticsService _analytics;
    private readonly UserManager<ApplicationUser> _userManager;

    public WellnessController(
        ApplicationDbContext db,
        BmiService bmiService,
        GamificationService gamification,
        PredictiveAnalyticsService analytics,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _bmiService = bmiService;
        _gamification = gamification;
        _analytics = analytics;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var points = await _db.Points.FirstOrDefaultAsync(p => p.UserId == user.Id);

        var badges = await _db.UserBadges
            .Where(ub => ub.UserId == user.Id)
            .Include(ub => ub.Badge)
            .Select(ub => ub.Badge!.Name)
            .ToListAsync();

        var recentLogs = await _db.WellnessLogs
            .Where(w => w.UserId == user.Id)
            .OrderByDescending(w => w.CreatedAt)
            .Take(30)
            .ToListAsync();

        var latestBmi = await _db.BmiRecords
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        var risk = await _analytics.ComputeUserRiskAsync(user.Id);

        var vm = new DashboardViewModel
        {
            FullName = user.FullName,
            TotalPoints = points?.Total ?? 0,
            Badges = badges,
            RecentLogs = recentLogs.OrderBy(l => l.CreatedAt).ToList(), // chronological for charting
            LatestBmi = latestBmi,
            RiskLevel = risk.Level.ToString(),
            RiskFactors = risk.Factors
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Log() => View(new WellnessLogViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Log(WellnessLogViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var bmiResult = _bmiService.Compute(model.WeightKg, model.HeightM);

        var log = new WellnessLog
        {
            UserId = user.Id,
            StressScore = model.StressScore,
            FatigueScore = model.FatigueScore,
            ActivityMins = model.ActivityMins,
            MentalScore = model.MentalScore,
            Bmi = bmiResult.Bmi
        };
        _db.WellnessLogs.Add(log);

        _db.BmiRecords.Add(new BmiRecord
        {
            UserId = user.Id,
            HeightM = model.HeightM,
            WeightKg = model.WeightKg,
            Bmi = bmiResult.Bmi,
            Category = bmiResult.Category
        });

        await _db.SaveChangesAsync();

        var newBadges = await _gamification.AwardPointsAndBadgesAsync(user.Id);

        TempData["BmiResult"] = $"{bmiResult.Bmi} — {bmiResult.Category}";
        TempData["NewBadges"] = newBadges.Count > 0
            ? string.Join(", ", newBadges.Select(b => b.Name))
            : null;

        return RedirectToAction("Dashboard");
    }
}
