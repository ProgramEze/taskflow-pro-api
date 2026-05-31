using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public Guid? AssignedUserId { get; set; }

    public User? AssignedUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}