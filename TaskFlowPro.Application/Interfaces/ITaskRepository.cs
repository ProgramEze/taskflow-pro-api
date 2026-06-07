using TaskFlowPro.Application.DTOs.Common;
using TaskFlowPro.Application.DTOs.Tasks;
using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task<List<TaskItem>> GetByProjectIdAsync(Guid projectId);

    Task<PagedResult<TaskItem>> GetByProjectIdPagedAsync(
        Guid projectId,
        TaskQueryParameters query
    );

    Task<TaskItem?> GetByIdAsync(Guid taskId);

    Task<List<TaskItem>> ObtenerTareasAsignadasAsync(Guid usuarioId);
}