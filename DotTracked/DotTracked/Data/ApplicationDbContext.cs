using DotTracked.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Issue> Issues { get; set; }
    public DbSet<Absence> Absences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Issue>().Property(i => i.Title).HasMaxLength(150);
        modelBuilder.Entity<Issue>().Property(i => i.Description).HasMaxLength(400);
        modelBuilder.Entity<Issue>()
            .HasOne(i => i.Creator)
            .WithMany(u => u.Issues)
            .HasForeignKey(i => i.CreatorId)
            .IsRequired();

        modelBuilder.Entity<Absence>().Property(a => a.Description).HasMaxLength(200);
        modelBuilder.Entity<Absence>()
            .HasOne(a => a.User)
            .WithMany(u => u.Absences)
            .HasForeignKey(a => a.UserId)
            .IsRequired();
    }
}