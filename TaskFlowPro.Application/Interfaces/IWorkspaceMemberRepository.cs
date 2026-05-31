using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface IWorkspaceMemberRepository
{
    Task AddAsync(WorkspaceMember member);

    Task UpdateAsync(WorkspaceMember member);

    Task<List<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId);

    Task<WorkspaceMember?> GetByIdAsync(Guid memberId);

    Task<WorkspaceMember?> GetByWorkspaceAndUserIdAsync(Guid workspaceId, Guid userId);
}