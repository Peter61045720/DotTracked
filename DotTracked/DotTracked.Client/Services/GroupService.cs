using System.Net;
using System.Net.Http.Json;
using System.Web;
using DotTracked.Shared.DTOs;
using MudBlazor;

namespace DotTracked.Client.Services;

public class GroupService(HttpClient http, ISnackbar snackbar) : IGroupService
{
    private static string GroupPath => "api/groups";

    public async Task<List<GroupDto>> GetGroupsAsync()
    {
        var response = await http.GetAsync(GroupPath);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<GroupDto>>() ?? [];
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
    {
        var response = await http.GetAsync($"{GroupPath}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GroupDto>();
    }

    public async Task<GroupDto> CreateGroupAsync(GroupDto groupDto)
    {
        var response = await http.PostAsJsonAsync(GroupPath, groupDto);

        response.EnsureSuccessStatusCode();

        var createdGroup = await response.Content.ReadFromJsonAsync<GroupDto>();

        return createdGroup ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateGroupAsync(Guid id, GroupDto groupDto)
    {
        var response = await http.PutAsJsonAsync($"{GroupPath}/{id}", groupDto);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to update this item", Severity.Error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGroupAsync(Guid id)
    {
        var response = await http.DeleteAsync($"{GroupPath}/{id}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to delete this item", Severity.Error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<UserDto>> GetGroupMembersAsync(Guid groupId)
    {
        var response = await http.GetAsync($"{MemberPath(groupId)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return [];
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? [];
    }

    public async Task<UserDto?> GetGroupMemberByEmailAsync(Guid groupId, string email)
    {
        var response = await http.GetAsync($"{MemberPath(groupId)}/details?email={HttpUtility.UrlEncode(email)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add($"The requested item was not found: {email}", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<UserDto?> CreateGroupMemberAsync(Guid groupId, string email)
    {
        var response = await http.PostAsync($"{MemberPath(groupId)}/?email={HttpUtility.UrlEncode(email)}", null);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to create this item", Severity.Error);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add($"The requested item was not found: {email}", Severity.Warning);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("This user is already a member of this group", Severity.Error);
            return null;
        }

        response.EnsureSuccessStatusCode();

        var member = await response.Content.ReadFromJsonAsync<UserDto>();

        return member ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task DeleteGroupMemberAsync(Guid groupId, string email)
    {
        var response = await http.DeleteAsync($"{MemberPath(groupId)}/?email={HttpUtility.UrlEncode(email)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to delete this item", Severity.Error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add($"The requested item was not found: {email}", Severity.Warning);
            return;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("A moderator cannot be deleted", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task LeaveGroupAsync(Guid groupId)
    {
        var response = await http.DeleteAsync($"{MemberPath(groupId)}/leave");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("You are not a member of this group", Severity.Warning);
            return;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("A moderator cannot be deleted", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<IssueDto>> GetGroupIssuesAsync(Guid groupId)
    {
        var response = await http.GetAsync($"{IssuePath(groupId)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return [];
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<IssueDto>>() ?? [];
    }

    public async Task<IssueDto?> GetGroupIssueByIdAsync(Guid groupId, Guid issueId)
    {
        var response = await http.GetAsync($"{IssuePath(groupId)}/{issueId}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IssueDto>();
    }

    public async Task<IssueDto?> CreateGroupIssueAsync(Guid groupId, IssueDto issueDto)
    {
        var response = await http.PostAsJsonAsync($"{IssuePath(groupId)}", issueDto);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to create this item", Severity.Error);
            return null;
        }

        response.EnsureSuccessStatusCode();

        var createdIssue = await response.Content.ReadFromJsonAsync<IssueDto>();

        return createdIssue ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateGroupIssueAsync(Guid groupId, Guid issueId, IssueDto issueDto)
    {
        var response = await http.PutAsJsonAsync($"{IssuePath(groupId)}/{issueId}", issueDto);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to update this item", Severity.Error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteGroupIssueAsync(Guid groupId, Guid issueId)
    {
        var response = await http.DeleteAsync($"{IssuePath(groupId)}/{issueId}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to delete this item", Severity.Error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<UserDto>> GetIssueAssigneesAsync(Guid groupId, Guid issueId)
    {
        var response = await http.GetAsync($"{AssignmentPath(groupId, issueId)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return [];
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? [];
    }

    public async Task<UserDto?> CreateAssignmentAsync(Guid groupId, Guid issueId, string email)
    {
        var response = await http.PostAsync($"{AssignmentPath(groupId, issueId)}/?email={HttpUtility.UrlEncode(email)}",
            null);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to create this item", Severity.Error);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add($"The requested item was not found: {email}", Severity.Warning);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            snackbar.Add("This user is already assigned to this issue", Severity.Error);
            return null;
        }

        response.EnsureSuccessStatusCode();

        var assignee = await response.Content.ReadFromJsonAsync<UserDto>();

        return assignee ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task DeleteAssignmentAsync(Guid groupId, Guid issueId, string email)
    {
        var response =
            await http.DeleteAsync($"{AssignmentPath(groupId, issueId)}/?email={HttpUtility.UrlEncode(email)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to delete this item", Severity.Error);
            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private static string MemberPath(Guid groupId)
    {
        return $"{GroupPath}/{groupId}/members";
    }

    private static string IssuePath(Guid groupId)
    {
        return $"{GroupPath}/{groupId}/issues";
    }

    private static string AssignmentPath(Guid groupId, Guid issueId)
    {
        return $"{IssuePath(groupId)}/{issueId}/assignments";
    }
}