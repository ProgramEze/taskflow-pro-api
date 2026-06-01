using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface IWorkspaceAuthorizationService
{
    WorkspaceMember GetCurrentMemberOrThrow(Workspace workspace, Guid currentUserId);

    void EnsureMember(Workspace workspace, Guid currentUserId);

    void EnsureOwner(Workspace workspace, Guid currentUserId);

    void EnsureOwnerOrAdmin(Workspace workspace, Guid currentUserId);
}