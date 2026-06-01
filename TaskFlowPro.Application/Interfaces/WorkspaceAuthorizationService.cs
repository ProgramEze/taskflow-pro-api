using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.Services;

public class WorkspaceAuthorizationService : IWorkspaceAuthorizationService
{
    public WorkspaceMember GetCurrentMemberOrThrow(Workspace workspace, Guid currentUserId)
    {
        var currentMember = workspace.Members.FirstOrDefault(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (currentMember is null)
            throw new ForbiddenException("No pertenecés a este workspace.");

        return currentMember;
    }

    public void EnsureMember(Workspace workspace, Guid currentUserId)
    {
        GetCurrentMemberOrThrow(workspace, currentUserId);
    }

    public void EnsureOwner(Workspace workspace, Guid currentUserId)
    {
        var currentMember = GetCurrentMemberOrThrow(workspace, currentUserId);

        if (currentMember.Role != WorkspaceRole.Owner)
            throw new ForbiddenException("Solo el Owner puede realizar esta acción.");
    }

    public void EnsureOwnerOrAdmin(Workspace workspace, Guid currentUserId)
    {
        var currentMember = GetCurrentMemberOrThrow(workspace, currentUserId);

        if (currentMember.Role is not WorkspaceRole.Owner and not WorkspaceRole.Admin)
            throw new ForbiddenException("Solo Owner o Admin pueden realizar esta acción.");
    }
}