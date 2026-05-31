namespace TaskFlowPro.Application.DTOs.Workspaces;

public class WorkspaceResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }
}