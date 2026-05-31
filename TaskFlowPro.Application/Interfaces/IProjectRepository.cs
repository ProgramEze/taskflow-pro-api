using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface IProjectRepository
{
    Task AddAsync(Project project);

    Task<List<Project>> GetByWorkspaceIdAsync(Guid workspaceId);

    Task<Project?> GetByIdAsync(Guid projectId);
}