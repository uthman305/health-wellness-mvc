using System.ComponentModel.DataAnnotations;

namespace HealthWellnessMVC.Models;

// One row per wellness log submission, covering all five indicators from the report.
public class WellnessLog
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Range(1, 10)]
    public int StressScore { get; set; }

    [Range(1, 10)]
    public int FatigueScore { get; set; }

    [Range(0, 1440)]
    public int ActivityMins { get; set; }

    [Range(1, 10)]
    public int MentalScore { get; set; }

    public double Bmi { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
