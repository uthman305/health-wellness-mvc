namespace HealthWellnessMVC.Models;

// Stores each computed BMI value alongside the height/weight that produced it.
public class BmiRecord
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public double HeightM { get; set; }
    public double WeightKg { get; set; }
    public double Bmi { get; set; }
    public string Category { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
