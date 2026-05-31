using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task<List<TaskItem>> GetByProjectIdAsync(Guid projectId);

    Task<TaskItem?> GetByIdAsync(Guid taskId);
}