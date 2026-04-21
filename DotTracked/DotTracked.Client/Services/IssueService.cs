using System.Net;
using System.Net.Http.Json;
using DotTracked.Shared.DTOs;
using MudBlazor;

namespace DotTracked.Client.Services;

public class IssueService(HttpClient http, ISnackbar snackbar) : IIssueService
{
    public async Task<List<IssueDto>> GetIssuesAsync()
    {
        var response = await http.GetAsync("api/issues");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<IssueDto>>() ?? [];
    }

    public async Task<List<IssueDto>> GetUpcomingIssuesAsync()
    {
        var response = await http.GetAsync("api/issues/upcoming");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<IssueDto>>() ?? [];
    }

    public async Task<IssueDto?> GetIssueByIdAsync(Guid id)
    {
        var response = await http.GetAsync($"api/issues/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IssueDto>();
    }

    public async Task<IssueDto> CreateIssueAsync(IssueDto issueDto)
    {
        var response = await http.PostAsJsonAsync("api/issues", issueDto);

        response.EnsureSuccessStatusCode();

        var createdIssue = await response.Content.ReadFromJsonAsync<IssueDto>();

        return createdIssue ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateIssueAsync(Guid id, IssueDto issueDto)
    {
        var response = await http.PutAsJsonAsync($"api/issues/{id}", issueDto);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to update this item", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteIssueAsync(Guid id)
    {
        var response = await http.DeleteAsync($"api/issues/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Info);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to delete this item", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }
}