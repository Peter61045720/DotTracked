using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface IIssueService
{
    Task<List<IssueDto>> GetIssuesAsync();
    Task<List<IssueDto>> GetUpcomingIssuesAsync();
    Task<IssueDto?> GetIssueByIdAsync(Guid id);
    Task<IssueDto> CreateIssueAsync(IssueDto issueDto);
    Task UpdateIssueAsync(Guid id, IssueDto issueDto);
    Task DeleteIssueAsync(Guid id);
}