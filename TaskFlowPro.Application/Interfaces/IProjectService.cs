using TaskFlowPro.Application.DTOs.Projects;

namespace TaskFlowPro.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(
        Guid currentUserId,
        Guid workspaceId,
        CreateProjectRequest request
    );

    Task<List<ProjectResponse>> GetByWorkspaceIdAsync(
        Guid currentUserId,
        Guid workspaceId
    );

    Task<ProjectResponse> GetByIdAsync(
        Guid currentUserId,
        Guid projectId
    );
}