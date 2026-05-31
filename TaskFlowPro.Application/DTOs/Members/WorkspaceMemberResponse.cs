using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Members;

public class WorkspaceMemberResponse
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public Guid UserId { get; set; }

    public string UserFullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public WorkspaceRole Role { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsActive { get; set; }
}