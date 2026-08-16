using HealthWellnessMVC.Data;
using HealthWellnessMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Services;

// Direct translation of award_points_and_badges from the project report:
// +10 points per log, badges unlocked at 1 / 7 / 14 / 30 / 60 total logs.
public class GamificationService
{
    private readonly ApplicationDbContext _db;
    private const int PointsPerLog = 10;

    public GamificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Badge>> AwardPointsAndBadgesAsync(string userId)
    {
        var points = await _db.Points.FirstOrDefaultAsync(p => p.UserId == userId);
        if (points is null)
        {
            points = new Points { UserId = userId, Total = 0 };
            _db.Points.Add(points);
        }
        points.Total += PointsPerLog;

        int logCount = await _db.WellnessLogs.CountAsync(w => w.UserId == userId);

        var alreadyEarnedBadgeIds = await _db.UserBadges
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync();

        var eligibleBadges = await _db.Badges
            .Where(b => b.LogCountThreshold <= logCount && !alreadyEarnedBadgeIds.Contains(b.Id))
            .ToListAsync();

        var newlyAwarded = new List<Badge>();
        foreach (var badge in eligibleBadges)
        {
            _db.UserBadges.Add(new UserBadge { UserId = userId, BadgeId = badge.Id });
            newlyAwarded.Add(badge);
        }

        await _db.SaveChangesAsync();
        return newlyAwarded;
    }

    // Standard badge set from the report: First Log, Week Streak, Fortnight, Monthly Hero, Wellness Star.
    public static readonly (string Name, int Threshold)[] DefaultBadges =
    {
        ("First Log", 1),
        ("Week Streak", 7),
        ("Fortnight", 14),
        ("Monthly Hero", 30),
        ("Wellness Star", 60),
    };
}
