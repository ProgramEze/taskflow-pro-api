using TaskFlowPro.Application.DTOs.Members;

namespace TaskFlowPro.Application.Interfaces;

public interface IWorkspaceMemberService
{
    Task<WorkspaceMemberResponse> AddMemberAsync(
        Guid currentUserId,
        Guid workspaceId,
        AddWorkspaceMemberRequest request
    );

    Task<List<WorkspaceMemberResponse>> GetMembersAsync(
        Guid currentUserId,
        Guid workspaceId
    );

    Task<WorkspaceMemberResponse> ChangeRoleAsync(
        Guid currentUserId,
        Guid workspaceId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request
    );

    Task RemoveMemberAsync(
        Guid currentUserId,
        Guid workspaceId,
        Guid memberId
    );
}