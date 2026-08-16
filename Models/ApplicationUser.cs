using Microsoft.AspNetCore.Identity;

namespace HealthWellnessMVC.Models;

// Extends Identity's built-in user with the extra field the report calls for (full name).
// Password hashing, email, and role membership are handled by ASP.NET Core Identity itself.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
