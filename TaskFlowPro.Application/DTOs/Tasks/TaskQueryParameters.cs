using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Tasks;

public class TaskQueryParameters
{
    public TaskItemStatus? Status { get; set; }

    public TaskPriority? Priority { get; set; }

    public Guid? AssignedUserId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}