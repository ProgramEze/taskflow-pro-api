using TaskFlowPro.Application.DTOs.Tasks;

namespace TaskFlowPro.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(
        Guid currentUserId,
        Guid projectId,
        CreateTaskRequest request
    );

    Task<List<TaskResponse>> GetByProjectIdAsync(
        Guid currentUserId,
        Guid projectId
    );

    Task<TaskResponse> GetByIdAsync(
        Guid currentUserId,
        Guid taskId
    );

    Task<TaskResponse> ChangeStatusAsync(
        Guid currentUserId,
        Guid taskId,
        UpdateTaskStatusRequest request
    );

    Task<TaskResponse> ChangePriorityAsync(
        Guid currentUserId,
        Guid taskId,
        UpdateTaskPriorityRequest request
    );

    Task<TaskResponse> UpdateAsync(
        Guid currentUserId,
        Guid taskId,
        UpdateTaskRequest request
    );

    Task DeleteAsync(
        Guid currentUserId,
        Guid taskId
    );

    Task<TaskResponse> AssignAsync(
        Guid currentUserId,
        Guid taskId,
        AssignTaskRequest request
    );
}