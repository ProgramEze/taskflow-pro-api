using TaskFlowPro.Application.DTOs.Workspaces;

namespace TaskFlowPro.Application.Interfaces;

public interface IWorkspaceService
{
    Task<WorkspaceResponse> CreateAsync(Guid currentUserId, CreateWorkspaceRequest request);

    Task<List<WorkspaceResponse>> GetMyWorkspacesAsync(Guid currentUserId);

    Task<WorkspaceResponse> GetByIdAsync(Guid currentUserId, Guid workspaceId);
}