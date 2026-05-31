using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }
}