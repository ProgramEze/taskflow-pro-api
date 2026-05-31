namespace TaskFlowPro.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }

    public Guid TaskItemId { get; set; }

    public TaskItem TaskItem { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}