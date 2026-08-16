using HealthWellnessMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthWellnessMVC.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<WellnessLog> WellnessLogs => Set<WellnessLog>();
    public DbSet<BmiRecord> BmiRecords => Set<BmiRecord>();
    public DbSet<Points> Points => Set<Points>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<MentorshipAssignment> MentorshipAssignments => Set<MentorshipAssignment>();
    public DbSet<WellnessCardSession> WellnessCardSessions => Set<WellnessCardSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Indexes on the columns the dashboard and leaderboard query most, per the report's design.
        builder.Entity<WellnessLog>()
            .HasIndex(w => new { w.UserId, w.CreatedAt });

        builder.Entity<BmiRecord>()
            .HasIndex(b => new { b.UserId, b.CreatedAt });

        builder.Entity<Points>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        builder.Entity<MentorshipAssignment>()
            .HasOne(m => m.Mentor)
            .WithMany()
            .HasForeignKey(m => m.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MentorshipAssignment>()
            .HasOne(m => m.Mentee)
            .WithMany()
            .HasForeignKey(m => m.MenteeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
