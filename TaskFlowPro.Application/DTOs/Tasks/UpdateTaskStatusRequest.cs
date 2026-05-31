using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.DTOs.Tasks;

public class UpdateTaskStatusRequest
{
    public TaskItemStatus Status { get; set; }
}