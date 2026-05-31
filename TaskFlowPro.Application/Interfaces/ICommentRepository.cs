using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface ICommentRepository
{
    Task AddAsync(Comment comment);

    Task UpdateAsync(Comment comment);

    Task<List<Comment>> GetByTaskIdAsync(Guid taskId);

    Task<Comment?> GetByIdAsync(Guid commentId);
}