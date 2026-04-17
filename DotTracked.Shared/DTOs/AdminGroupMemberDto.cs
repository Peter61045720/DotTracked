namespace DotTracked.Shared.DTOs;

public class AdminGroupMemberDto
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsModerator { get; set; }
    public DateTime JoinedAt { get; set; }
}