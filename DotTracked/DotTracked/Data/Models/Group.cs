namespace DotTracked.Data.Models;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Issue> Issues { get; } = new List<Issue>();

    public ICollection<ApplicationUser> Members { get; } = new List<ApplicationUser>();
    public ICollection<GroupMember> GroupMembers { get; } = new List<GroupMember>();
}