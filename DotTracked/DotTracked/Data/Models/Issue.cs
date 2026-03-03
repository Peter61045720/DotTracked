using DotTracked.Shared.Enums;

namespace DotTracked.Data.Models;

public class Issue
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string CreatorId { get; set; }
    public ApplicationUser Creator { get; set; } = null!;
}