namespace HealthWellnessMVC.Services;

public record BmiResult(double Bmi, string Category);

// Direct translation of bmi_utils.py's compute_bmi from the project report.
public class BmiService
{
    public BmiResult Compute(double weightKg, double heightM)
    {
        double bmi = Math.Round(weightKg / (heightM * heightM), 1);

        string category = bmi switch
        {
            < 18.5 => "Underweight",
            < 25.0 => "Normal weight",
            < 30.0 => "Overweight",
            _ => "Obese"
        };

        return new BmiResult(bmi, category);
    }
}
