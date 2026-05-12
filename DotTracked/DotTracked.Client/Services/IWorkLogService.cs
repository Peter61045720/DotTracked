using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface IWorkLogService
{
    Task<List<WorkLogDto>> GetUserWorkLogsAsync();
    Task<List<WorkLogDto>> GetWorkLogsAsync(Guid issueId);
    Task<int> GetTotalLoggedTimeAsync(Guid issueId);
    Task<WorkLogDto?> GetWorkLogByIdAsync(Guid issueId, Guid workLogId);
    Task<WorkLogDto> CreateWorkLogAsync(Guid issueId, WorkLogDto workLogDto);
    Task UpdateWorkLogAsync(Guid issueId, Guid workLogId, WorkLogDto workLogDto);
    Task DeleteWorkLogAsync(Guid issueId, Guid workLogId);
}