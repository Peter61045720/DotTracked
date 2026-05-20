using DotTracked.Shared.Enums;

namespace DotTracked.Shared.DTOs;

public class IssueDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public int EstimatedSeconds { get; set; }
}