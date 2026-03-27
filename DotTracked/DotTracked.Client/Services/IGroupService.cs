using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface IGroupService
{
    Task<List<GroupDto>> GetGroupsAsync();
    Task<GroupDto?> GetGroupByIdAsync(Guid id);
    Task<GroupDto> CreateGroupAsync(GroupDto groupDto);
    Task UpdateGroupAsync(Guid id, GroupDto groupDto);
    Task DeleteGroupAsync(Guid id);

    Task<List<UserDto>> GetGroupMembersAsync(Guid groupId);
    Task<UserDto?> GetGroupMemberByEmailAsync(Guid groupId, string email);
    Task<UserDto> CreateGroupMemberAsync(Guid groupId, string email);
    Task DeleteGroupMemberAsync(Guid groupId, string email);
    Task LeaveGroupAsync(Guid groupId);

    Task<List<IssueDto>> GetGroupIssuesAsync(Guid groupId);
    Task<IssueDto?> GetGroupIssueByIdAsync(Guid groupId, Guid issueId);
    Task<IssueDto> CreateGroupIssueAsync(Guid groupId, IssueDto issueDto);
    Task UpdateGroupIssueAsync(Guid groupId, Guid issueId, IssueDto issueDto);
    Task DeleteGroupIssueAsync(Guid groupId, Guid issueId);

    Task<List<UserDto>> GetIssueAssigneesAsync(Guid groupId, Guid issueId);
    Task<UserDto> CreateAssignment(Guid groupId, Guid issueId, string email);
    Task DeleteAssignment(Guid groupId, Guid issueId, string email);
}