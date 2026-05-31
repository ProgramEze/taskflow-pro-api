using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Members;

public class UpdateWorkspaceMemberRoleRequest
{
    public WorkspaceRole Role { get; set; }
}