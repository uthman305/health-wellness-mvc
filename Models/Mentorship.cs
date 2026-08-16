namespace HealthWellnessMVC.Models;

// Pairing of a mentor (lecturer, typically) with a mentee (student), created by an Administrator.
public class MentorshipAssignment
{
    public int Id { get; set; }
    public string MentorId { get; set; } = string.Empty;
    public ApplicationUser? Mentor { get; set; }
    public string MenteeId { get; set; } = string.Empty;
    public ApplicationUser? Mentee { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public List<WellnessCardSession> Sessions { get; set; } = new();
}

// Individual mentorship session record, tied to a specific wellness dimension.
public class WellnessCardSession
{
    public int Id { get; set; }
    public int MentorshipAssignmentId { get; set; }
    public MentorshipAssignment? MentorshipAssignment { get; set; }

    public DateTime SessionDate { get; set; } = DateTime.UtcNow;
    public string WellnessDimension { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
