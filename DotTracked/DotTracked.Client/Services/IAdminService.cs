using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto?> GetUserByIdAsync(string id);
    Task DeleteUserAsync(string id);

    Task<List<AdminGroupDto>> GetGroupsAsync();
    Task<AdminGroupDto?> GetGroupByIdAsync(Guid id);
    Task DeleteGroupAsync(Guid id);

    Task<List<AdminGroupMemberDto>> GetGroupMembersAsync(Guid groupId);
    Task<AdminGroupMemberDto?> GetGroupMemberByIdAsync(Guid groupId, string userId);
    Task<AdminGroupMemberDto?> CreateGroupMemberAsync(Guid groupId, string userId);
    Task UpdateGroupModeratorStatusAsync(Guid groupId, string userId, bool isModerator);
    Task DeleteGroupMemberAsync(Guid groupId, string userId);
}