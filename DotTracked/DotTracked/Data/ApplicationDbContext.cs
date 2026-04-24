using DotTracked.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Issue> Issues { get; set; }
    public DbSet<WorkLog> WorkLogs { get; set; }
    public DbSet<Absence> Absences { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Assignment> Assignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Issue>().Property(i => i.Title).HasMaxLength(150);
        modelBuilder.Entity<Issue>().Property(i => i.Description).HasMaxLength(400);
        modelBuilder.Entity<Issue>()
            .HasOne(i => i.Creator)
            .WithMany(u => u.CreatedIssues)
            .HasForeignKey(i => i.CreatorId)
            .IsRequired(false);
        modelBuilder.Entity<Issue>()
            .HasMany(i => i.Assignees)
            .WithMany(u => u.AssignedIssues)
            .UsingEntity<Assignment>(
                r => r.HasOne<ApplicationUser>(e => e.User).WithMany(e => e.Assignments).HasForeignKey(e => e.UserId),
                l => l.HasOne<Issue>(e => e.Issue).WithMany(e => e.Assignments).HasForeignKey(e => e.IssueId),
                j => j.HasKey(e => new { e.IssueId, e.UserId }));

        modelBuilder.Entity<WorkLog>(b =>
        {
            b.Property(w => w.Description).HasMaxLength(400);

            b.HasOne(w => w.User).WithMany(u => u.WorkLogs).HasForeignKey(w => w.UserId).IsRequired();
            b.HasOne(w => w.Issue).WithMany(i => i.WorkLogs).HasForeignKey(w => w.IssueId).IsRequired();
        });

        modelBuilder.Entity<Absence>().Property(a => a.Description).HasMaxLength(200);
        modelBuilder.Entity<Absence>()
            .HasOne(a => a.User)
            .WithMany(u => u.Absences)
            .HasForeignKey(a => a.UserId)
            .IsRequired();

        modelBuilder.Entity<Group>()
            .HasMany(g => g.Issues)
            .WithOne(i => i.Group)
            .HasForeignKey(i => i.GroupId)
            .IsRequired(false);
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Members)
            .WithMany(u => u.Groups)
            .UsingEntity<GroupMember>(
                r => r.HasOne<ApplicationUser>(e => e.User).WithMany(e => e.GroupMembers).HasForeignKey(e => e.UserId),
                l => l.HasOne<Group>(e => e.Group).WithMany(e => e.GroupMembers).HasForeignKey(e => e.GroupId),
                j => j.HasKey(e => new { e.GroupId, e.UserId }));
    }
}