using System.ComponentModel.DataAnnotations;

namespace HealthWellnessMVC.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string MatricNumber { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string College { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student"; // Student | Lecturer

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class WellnessLogViewModel
{
    [Range(1, 10)]
    public int StressScore { get; set; }

    [Range(1, 10)]
    public int FatigueScore { get; set; }

    [Range(0, 1440)]
    public int ActivityMins { get; set; }

    [Range(1, 10)]
    public int MentalScore { get; set; }

    [Range(0.5, 3.0)]
    public double HeightM { get; set; }

    [Range(1, 400)]
    public double WeightKg { get; set; }
}

public class DashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public List<string> Badges { get; set; } = new();
    public List<WellnessLog> RecentLogs { get; set; } = new();
    public BmiRecord? LatestBmi { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public List<string> RiskFactors { get; set; } = new();
}

public class LeaderboardRowViewModel
{
    public string FullName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Rank { get; set; }
}

public class MentorshipAssignViewModel
{
    public List<(string Id, string Name)> Mentors { get; set; } = new();
    public List<(string Id, string Name)> Mentees { get; set; } = new();

    [Required]
    public string MentorId { get; set; } = string.Empty;

    [Required]
    public string MenteeId { get; set; } = string.Empty;
}

public class WellnessCardSessionViewModel
{
    public int MentorshipAssignmentId { get; set; }

    [Required]
    public string WellnessDimension { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalLecturers { get; set; }
    public double AverageStress { get; set; }
    public double AverageFatigue { get; set; }
    public double AverageMentalScore { get; set; }
    public int UnderweightCount { get; set; }
    public int NormalCount { get; set; }
    public int OverweightCount { get; set; }
    public int ObeseCount { get; set; }
    public int DailyActiveUsers { get; set; }
    public int LowRiskCount { get; set; }
    public int ModerateRiskCount { get; set; }
    public int HighRiskCount { get; set; }
    public List<string> TrendLabels { get; set; } = new();
    public List<double> TrendValues { get; set; } = new();
}
