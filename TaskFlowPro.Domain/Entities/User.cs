namespace TaskFlowPro.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();

    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}