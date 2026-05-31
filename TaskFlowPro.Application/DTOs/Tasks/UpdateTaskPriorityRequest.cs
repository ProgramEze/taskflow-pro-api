using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Tasks;

public class UpdateTaskPriorityRequest
{
    public TaskPriority Priority { get; set; }
}