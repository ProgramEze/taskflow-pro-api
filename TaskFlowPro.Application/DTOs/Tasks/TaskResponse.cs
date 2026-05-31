using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Tasks;

public class TaskResponse
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid? AssignedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsActive { get; set; }
}