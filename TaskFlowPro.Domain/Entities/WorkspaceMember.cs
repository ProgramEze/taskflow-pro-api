using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Domain.Entities;

public class WorkspaceMember
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}