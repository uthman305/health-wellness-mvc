namespace HealthWellnessMVC.Models;

// Running points balance per user, incremented on every log submission.
public class Points
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public int Total { get; set; }
}

// Available badge definitions and the log-count threshold required to unlock each one.
public class Badge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LogCountThreshold { get; set; }
}

// Junction table: which badges each user has earned, and when.
public class UserBadge
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public int BadgeId { get; set; }
    public Badge? Badge { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
}
