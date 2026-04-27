namespace DotTracked.Data.Models;

public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
}