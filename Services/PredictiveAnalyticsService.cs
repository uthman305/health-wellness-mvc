using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Services;

public enum RiskLevel { Low, Moderate, High }

public record UserRiskResult(RiskLevel Level, int Score, List<string> Factors);

public record PopulationRiskSummary(
    int LowCount,
    int ModerateCount,
    int HighCount,
    List<(string Label, double AvgStress)> StressTrend
);

// Rule-based predictive analytics: flags rising risk from trend direction in a user's
// own recent logs, rather than a single snapshot value. This is a heuristic scoring
// model (not a trained ML model), which keeps it transparent and auditable — every
// flagged factor can be explained in plain language to the student or admin viewing it.
public class PredictiveAnalyticsService
{
    private readonly ApplicationDbContext _db;
    private const int LookbackLogs = 10;

    public PredictiveAnalyticsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserRiskResult> ComputeUserRiskAsync(string userId)
    {
        var logs = await _db.WellnessLogs
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Take(LookbackLogs)
            .ToListAsync();

        if (logs.Count < 3)
            return new UserRiskResult(RiskLevel.Low, 0, new List<string> { "Not enough logs yet for a risk assessment (need at least 3)." });

        logs.Reverse(); // chronological order for trend comparison

        int score = 0;
        var factors = new List<string>();

        // Split into earlier vs later half to detect direction of change, not just current level.
        int mid = logs.Count / 2;
        var earlier = logs.Take(mid).ToList();
        var later = logs.Skip(mid).ToList();

        double stressDelta = later.Average(l => l.StressScore) - earlier.Average(l => l.StressScore);
        double fatigueDelta = later.Average(l => l.FatigueScore) - earlier.Average(l => l.FatigueScore);
        double mentalDelta = later.Average(l => l.MentalScore) - earlier.Average(l => l.MentalScore);
        double avgActivity = logs.Average(l => l.ActivityMins);
        double avgStress = logs.Average(l => l.StressScore);
        double avgFatigue = logs.Average(l => l.FatigueScore);
        double avgMental = logs.Average(l => l.MentalScore);

        if (stressDelta >= 1.5) { score += 2; factors.Add("Stress has been trending upward recently."); }
        if (avgStress >= 7.5) { score += 2; factors.Add("Average stress level is high (7.5+/10)."); }

        if (fatigueDelta >= 1.5) { score += 2; factors.Add("Fatigue has been trending upward recently."); }
        if (avgFatigue >= 7.5) { score += 1; factors.Add("Average fatigue level is high."); }

        if (mentalDelta <= -1.5) { score += 2; factors.Add("Mental health score has been declining recently."); }
        if (avgMental <= 3.5) { score += 2; factors.Add("Average mental health score is low."); }

        if (avgActivity < 20) { score += 1; factors.Add("Physical activity has been consistently low."); }

        var latestBmi = await _db.BmiRecords
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
        if (latestBmi != null && (latestBmi.Category == "Obese" || latestBmi.Category == "Underweight"))
        {
            score += 1;
            factors.Add($"Latest BMI falls in the '{latestBmi.Category}' range.");
        }

        var level = score >= 6 ? RiskLevel.High : score >= 3 ? RiskLevel.Moderate : RiskLevel.Low;
        if (factors.Count == 0) factors.Add("No elevated risk factors detected in recent logs.");

        return new UserRiskResult(level, score, factors);
    }

    // Anonymised, aggregate-only summary for the Administrator dashboard.
    // No individual scores or names are exposed — consistent with the rest of the admin view.
    public async Task<PopulationRiskSummary> ComputePopulationRiskAsync(IEnumerable<string> userIds)
    {
        int low = 0, moderate = 0, high = 0;
        foreach (var id in userIds)
        {
            var result = await ComputeUserRiskAsync(id);
            switch (result.Level)
            {
                case RiskLevel.Low: low++; break;
                case RiskLevel.Moderate: moderate++; break;
                case RiskLevel.High: high++; break;
            }
        }

        var cutoff = DateTime.UtcNow.AddDays(-14);
        var trend = await _db.WellnessLogs
            .Where(l => l.CreatedAt >= cutoff)
            .GroupBy(l => l.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Avg = g.Average(l => l.StressScore) })
            .OrderBy(g => g.Date)
            .ToListAsync();

        var stressTrend = trend.Select(t => (t.Date.ToString("MMM d"), t.Avg)).ToList();

        return new PopulationRiskSummary(low, moderate, high, stressTrend);
    }
}
