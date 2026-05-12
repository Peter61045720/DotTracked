using System.Net;
using System.Net.Http.Json;
using DotTracked.Shared.DTOs;
using MudBlazor;

namespace DotTracked.Client.Services;

public class WorkLogService(HttpClient http, ISnackbar snackbar) : IWorkLogService
{
    private static string BaseWorkLogPath => "api/worklogs";

    public async Task<List<WorkLogDto>> GetUserWorkLogsAsync()
    {
        var response = await http.GetAsync(BaseWorkLogPath);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WorkLogDto>>() ?? [];
    }

    public async Task<List<WorkLogDto>> GetWorkLogsAsync(Guid issueId)
    {
        var response = await http.GetAsync(WorkLogPath(issueId));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WorkLogDto>>() ?? [];
    }

    public async Task<int> GetTotalLoggedTimeAsync(Guid issueId)
    {
        var response = await http.GetAsync($"{WorkLogPath(issueId)}/total");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<WorkLogDto?> GetWorkLogByIdAsync(Guid issueId, Guid workLogId)
    {
        var response = await http.GetAsync($"{WorkLogPath(issueId)}/{workLogId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WorkLogDto>();
    }

    public async Task<WorkLogDto> CreateWorkLogAsync(Guid issueId, WorkLogDto workLogDto)
    {
        var response = await http.PostAsJsonAsync(WorkLogPath(issueId), workLogDto);

        response.EnsureSuccessStatusCode();

        var createdWorkLog = await response.Content.ReadFromJsonAsync<WorkLogDto>();

        return createdWorkLog ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateWorkLogAsync(Guid issueId, Guid workLogId, WorkLogDto workLogDto)
    {
        var response = await http.PutAsJsonAsync($"{WorkLogPath(issueId)}/{workLogId}", workLogDto);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteWorkLogAsync(Guid issueId, Guid workLogId)
    {
        var response = await http.DeleteAsync($"{WorkLogPath(issueId)}/{workLogId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Info);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private static string WorkLogPath(Guid issueId)
    {
        return $"api/issues/{issueId}/worklogs";
    }
}