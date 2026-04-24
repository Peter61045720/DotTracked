namespace DotTracked.Shared.DTOs;

public class WorkLogDto
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public int TimeSpentSeconds { get; set; }
    public DateTime DateOfLogging { get; set; }
    public bool IsOwner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}