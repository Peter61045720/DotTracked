using DotTracked.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace DotTracked.Data;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Issue> CreatedIssues { get; } = new List<Issue>();

    public ICollection<Absence> Absences { get; } = new List<Absence>();

    public ICollection<Group> Groups { get; } = new List<Group>();
    public ICollection<GroupMember> GroupMembers { get; } = new List<GroupMember>();

    public ICollection<Issue> AssignedIssues { get; } = new List<Issue>();
    public ICollection<Assignment> Assignments { get; } = new List<Assignment>();
}