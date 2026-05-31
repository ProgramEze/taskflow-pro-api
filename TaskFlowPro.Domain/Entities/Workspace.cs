namespace TaskFlowPro.Domain.Entities;

public class Workspace
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}