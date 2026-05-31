namespace TaskFlowPro.Application.DTOs.Comments;

public class CommentResponse
{
    public Guid Id { get; set; }

    public Guid TaskItemId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }
}