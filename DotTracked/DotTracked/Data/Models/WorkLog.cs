namespace DotTracked.Data.Models;

public class WorkLog
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public int TimeSpentSeconds { get; set; }
    public DateTime DateOfLogging { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
}