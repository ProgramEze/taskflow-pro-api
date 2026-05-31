using TaskFlowPro.Application.DTOs.Comments;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;

    public CommentService(
        ICommentRepository commentRepository,
        ITaskRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
    }

    public async Task<CommentResponse> CreateAsync(
        Guid currentUserId,
        Guid taskId,
        CreateCommentRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        var isMember = task.Project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para comentar esta tarea.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("El contenido del comentario es obligatorio.");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            UserId = currentUserId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        await _commentRepository.AddAsync(comment);

        return ToResponse(comment);
    }

    public async Task<List<CommentResponse>> GetByTaskIdAsync(
        Guid currentUserId,
        Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        var isMember = task.Project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para ver comentarios de esta tarea.");

        var comments = await _commentRepository.GetByTaskIdAsync(taskId);

        return comments
            .Select(ToResponse)
            .ToList();
    }

    public async Task<CommentResponse> UpdateAsync(
        Guid currentUserId,
        Guid commentId,
        UpdateCommentRequest request)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);

        if (comment is null || !comment.IsActive)
            throw new NotFoundException("Comentario no encontrado.");

        if (comment.UserId != currentUserId)
            throw new ForbiddenException("Solo el autor puede editar este comentario.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("El contenido del comentario es obligatorio.");

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);

        return ToResponse(comment);
    }

    public async Task DeleteAsync(Guid currentUserId, Guid commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);

        if (comment is null || !comment.IsActive)
            throw new NotFoundException("Comentario no encontrado.");

        if (comment.UserId != currentUserId)
            throw new ForbiddenException("Solo el autor puede eliminar este comentario.");

        comment.IsActive = false;
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);
    }

    private static CommentResponse ToResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            TaskItemId = comment.TaskItemId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsActive = comment.IsActive
        };
    }
}