using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Projects;

public class ProjectResponse
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }
}