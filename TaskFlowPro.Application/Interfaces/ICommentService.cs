using TaskFlowPro.Application.DTOs.Comments;

namespace TaskFlowPro.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponse> CreateAsync(
        Guid currentUserId,
        Guid taskId,
        CreateCommentRequest request
    );

    Task<List<CommentResponse>> GetByTaskIdAsync(
        Guid currentUserId,
        Guid taskId
    );

    Task<CommentResponse> UpdateAsync(
        Guid currentUserId,
        Guid commentId,
        UpdateCommentRequest request
    );

    Task DeleteAsync(
        Guid currentUserId,
        Guid commentId
    );
}