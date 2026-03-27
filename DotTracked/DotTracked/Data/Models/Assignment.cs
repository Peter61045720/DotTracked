namespace DotTracked.Data.Models;

public class Assignment
{
    public Guid IssueId { get; set; }
    public string UserId { get; set; }
    public Issue Issue { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
}