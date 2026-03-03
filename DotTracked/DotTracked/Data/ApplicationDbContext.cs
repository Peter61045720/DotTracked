using DotTracked.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Issue> Issues { get; set; }

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
    }
}