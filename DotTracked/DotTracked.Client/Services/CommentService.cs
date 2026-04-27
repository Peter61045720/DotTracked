using System.Net;
using System.Net.Http.Json;
using DotTracked.Shared.DTOs;
using MudBlazor;

namespace DotTracked.Client.Services;

public class CommentService(HttpClient http, ISnackbar snackbar) : ICommentService
{
    public async Task<List<CommentDto>> GetCommentsAsync(Guid issueId)
    {
        var response = await http.GetAsync(CommentPath(issueId));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CommentDto>>() ?? [];
    }

    public async Task<int> GetCommentCountAsync(Guid issueId)
    {
        var response = await http.GetAsync($"{CommentPath(issueId)}/count");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<CommentDto?> GetCommentByIdAsync(Guid issueId, Guid commentId)
    {
        var response = await http.GetAsync($"{CommentPath(issueId)}/{commentId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommentDto>();
    }

    public async Task<CommentDto> CreateCommentAsync(Guid issueId, CommentDto commentDto)
    {
        var response = await http.PostAsJsonAsync(CommentPath(issueId), commentDto);

        response.EnsureSuccessStatusCode();

        var createdComment = await response.Content.ReadFromJsonAsync<CommentDto>();

        return createdComment ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateCommentAsync(Guid issueId, Guid commentId, CommentDto commentDto)
    {
        var response = await http.PutAsJsonAsync($"{CommentPath(issueId)}/{commentId}", commentDto);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCommentAsync(Guid issueId, Guid commentId)
    {
        var response = await http.DeleteAsync($"{CommentPath(issueId)}/{commentId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Info);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private static string CommentPath(Guid issueId)
    {
        return $"api/issues/{issueId}/comments";
    }
}