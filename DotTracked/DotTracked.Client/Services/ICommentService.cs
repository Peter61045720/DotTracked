using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentsAsync(Guid issueId);
    Task<int> GetCommentCountAsync(Guid issueId);
    Task<CommentDto?> GetCommentByIdAsync(Guid issueId, Guid commentId);
    Task<CommentDto> CreateCommentAsync(Guid issueId, CommentDto commentDto);
    Task UpdateCommentAsync(Guid issueId, Guid commentId, CommentDto commentDto);
    Task DeleteCommentAsync(Guid issueId, Guid commentId);
}