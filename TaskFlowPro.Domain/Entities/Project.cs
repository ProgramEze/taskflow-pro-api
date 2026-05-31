using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}