namespace DotTracked.Data.Models;

public class GroupMember
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; }
    public Group Group { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public bool IsModerator { get; set; }
    public DateTime JoinedAt { get; set; }
}