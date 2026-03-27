namespace DotTracked.Shared.DTOs;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsModerator { get; set; }
    public DateTime JoinedAt { get; set; }
    public int MemberCount { get; set; }
}