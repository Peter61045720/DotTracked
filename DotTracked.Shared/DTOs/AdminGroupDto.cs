namespace DotTracked.Shared.DTOs;

public class AdminGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}