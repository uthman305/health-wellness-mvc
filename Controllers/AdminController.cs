using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models.ViewModels;
using HealthWellnessMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Controllers;

// Administrator Reporting — anonymised aggregate view only. No individual records exposed.
[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly PredictiveAnalyticsService _analytics;

    public AdminController(ApplicationDbContext db, UserManager<Models.ApplicationUser> userManager, PredictiveAnalyticsService analytics)
    {
        _db = db;
        _userManager = userManager;
        _analytics = analytics;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var students = await _userManager.GetUsersInRoleAsync("Student");
        var lecturers = await _userManager.GetUsersInRoleAsync("Lecturer");

        var logs = _db.WellnessLogs.AsQueryable();
        var bmiRecords = _db.BmiRecords.AsQueryable();

        var allUserIds = students.Select(s => s.Id).Concat(lecturers.Select(l => l.Id));
        var riskSummary = await _analytics.ComputePopulationRiskAsync(allUserIds);

        var vm = new AdminDashboardViewModel
        {
            TotalUsers = students.Count + lecturers.Count,
            TotalStudents = students.Count,
            TotalLecturers = lecturers.Count,
            AverageStress = await logs.AnyAsync() ? await logs.AverageAsync(l => l.StressScore) : 0,
            AverageFatigue = await logs.AnyAsync() ? await logs.AverageAsync(l => l.FatigueScore) : 0,
            AverageMentalScore = await logs.AnyAsync() ? await logs.AverageAsync(l => l.MentalScore) : 0,
            UnderweightCount = await bmiRecords.CountAsync(b => b.Category == "Underweight"),
            NormalCount = await bmiRecords.CountAsync(b => b.Category == "Normal weight"),
            OverweightCount = await bmiRecords.CountAsync(b => b.Category == "Overweight"),
            ObeseCount = await bmiRecords.CountAsync(b => b.Category == "Obese"),
            DailyActiveUsers = await logs
                .Where(l => l.CreatedAt.Date == DateTime.UtcNow.Date)
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync(),
            LowRiskCount = riskSummary.LowCount,
            ModerateRiskCount = riskSummary.ModerateCount,
            HighRiskCount = riskSummary.HighCount,
            TrendLabels = riskSummary.StressTrend.Select(t => t.Label).ToList(),
            TrendValues = riskSummary.StressTrend.Select(t => t.AvgStress).ToList()
        };

        return View(vm);
    }
}
