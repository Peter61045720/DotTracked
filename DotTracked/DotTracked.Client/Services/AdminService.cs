using System.Net;
using System.Net.Http.Json;
using DotTracked.Shared.DTOs;
using MudBlazor;

namespace DotTracked.Client.Services;

public class AdminService(HttpClient http, ISnackbar snackbar) : IAdminService
{
    private static string AdminPath => "api/admin";
    private static string UserPath => $"{AdminPath}/users";
    private static string GroupPath => $"{AdminPath}/groups";

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        var response = await http.GetAsync(UserPath);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AdminUserDto>>() ?? [];
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(string id)
    {
        var response = await http.GetAsync($"{UserPath}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdminUserDto>();
    }

    public async Task DeleteUserAsync(string id)
    {
        var response = await http.DeleteAsync($"{UserPath}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Warning);
            return;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("An error occured while deleting the user", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AdminGroupDto>> GetGroupsAsync()
    {
        var response = await http.GetAsync($"{GroupPath}");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AdminGroupDto>>() ?? [];
    }

    public async Task<AdminGroupDto?> GetGroupByIdAsync(Guid id)
    {
        var response = await http.GetAsync($"{GroupPath}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdminGroupDto>();
    }

    public async Task DeleteGroupAsync(Guid id)
    {
        var response = await http.DeleteAsync($"{GroupPath}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AdminGroupMemberDto>> GetGroupMembersAsync(Guid groupId)
    {
        var response = await http.GetAsync($"{MemberPath(groupId)}");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AdminGroupMemberDto>>() ?? [];
    }

    public async Task<AdminGroupMemberDto?> GetGroupMemberByIdAsync(Guid groupId, string userId)
    {
        var response = await http.GetAsync($"{MemberPath(groupId)}/{userId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdminGroupMemberDto>();
    }

    public async Task<AdminGroupMemberDto?> CreateGroupMemberAsync(Guid groupId, string userId)
    {
        var response = await http.PostAsync($"{MemberPath(groupId)}/{userId}", null);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("This user is already a member of this group", Severity.Error);
            return null;
        }

        response.EnsureSuccessStatusCode();

        var member = await response.Content.ReadFromJsonAsync<AdminGroupMemberDto>();

        return member ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateGroupModeratorStatusAsync(Guid groupId, string userId, bool isModerator)
    {
        var response = await http.PutAsync($"{MemberPath(groupId)}/{userId}/moderator?isModerator={isModerator}", null);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGroupMemberAsync(Guid groupId, string userId)
    {
        var response = await http.DeleteAsync($"{MemberPath(groupId)}/{userId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("A moderator cannot be deleted", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private static string MemberPath(Guid groupId)
    {
        return $"{GroupPath}/{groupId}/members";
    }
}