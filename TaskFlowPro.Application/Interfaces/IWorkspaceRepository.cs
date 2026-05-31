using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface IWorkspaceRepository
{
    Task AddAsync(Workspace workspace);

    Task<List<Workspace>> GetByUserIdAsync(Guid userId);

    Task<Workspace?> GetByIdAsync(Guid workspaceId);
}