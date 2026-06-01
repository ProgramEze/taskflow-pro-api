using TaskFlowPro.Application.DTOs.Tasks;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;
using TaskFlowPro.Application.DTOs.Common;

namespace TaskFlowPro.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<TaskResponse> CreateAsync(
        Guid currentUserId,
        Guid projectId,
        CreateTaskRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);

        if (project is null)
            throw new NotFoundException("Proyecto no encontrado.");

        if (project.Workspace is null || !project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        if (project.Status == ProjectStatus.Archived)
            throw new BadRequestException("No se pueden crear tareas en un proyecto archivado.");

        var isMember = project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para crear tareas en este proyecto.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("El título de la tarea es obligatorio.");

        if (!Enum.IsDefined(typeof(TaskPriority), request.Priority))
            throw new BadRequestException("Prioridad de tarea inválida.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = TaskItemStatus.Pending,
            Priority = request.Priority,
            CreatedByUserId = currentUserId,
            AssignedUserId = null,
            CreatedAt = DateTime.UtcNow,
            DueDate = request.DueDate,
            IsActive = true
        };

        await _taskRepository.AddAsync(task);

        return ToResponse(task);
    }

    public async Task<PagedResponse<TaskResponse>> GetByProjectIdAsync(
    Guid currentUserId,
    Guid projectId,
    TaskQueryParameters query)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);

        if (project is null)
            throw new NotFoundException("Proyecto no encontrado.");

        if (project.Workspace is null || !project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        var isMember = project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para ver tareas de este proyecto.");

        if (query.PageNumber < 1)
            throw new BadRequestException("El número de página debe ser mayor o igual a 1.");

        if (query.PageSize < 1)
            throw new BadRequestException("El tamaño de página debe ser mayor o igual a 1.");

        if (query.PageSize > 100)
            throw new BadRequestException("El tamaño de página no puede superar 100 elementos.");

        if (query.Status.HasValue && !Enum.IsDefined(typeof(TaskItemStatus), query.Status.Value))
            throw new BadRequestException("Estado de tarea inválido.");

        if (query.Priority.HasValue && !Enum.IsDefined(typeof(TaskPriority), query.Priority.Value))
            throw new BadRequestException("Prioridad de tarea inválida.");

        var pagedResult = await _taskRepository.GetByProjectIdPagedAsync(
            projectId,
            query
        );

        var totalPages = (int)Math.Ceiling(
            pagedResult.TotalItems / (double)query.PageSize
        );

        return new PagedResponse<TaskResponse>
        {
            Items = pagedResult.Items
                .Select(ToResponse)
                .ToList(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalItems = pagedResult.TotalItems,
            TotalPages = totalPages
        };
    }

    public async Task<TaskResponse> GetByIdAsync(
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
            throw new ForbiddenException("No tenés permiso para acceder a esta tarea.");

        return ToResponse(task);
    }

    public async Task<TaskResponse> ChangeStatusAsync(
        Guid currentUserId,
        Guid taskId,
        UpdateTaskStatusRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        if (task.Project.Status == ProjectStatus.Archived)
            throw new BadRequestException("No se puede modificar una tarea de un proyecto archivado.");

        var isMember = task.Project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para modificar esta tarea.");

        if (!Enum.IsDefined(typeof(TaskItemStatus), request.Status))
            throw new BadRequestException("Estado de tarea inválido.");

        task.Status = request.Status;

        await _taskRepository.UpdateAsync(task);

        return ToResponse(task);
    }

    public async Task<TaskResponse> ChangePriorityAsync(
        Guid currentUserId,
        Guid taskId,
        UpdateTaskPriorityRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        if (task.Project.Status == ProjectStatus.Archived)
            throw new BadRequestException("No se puede modificar una tarea de un proyecto archivado.");

        var isMember = task.Project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para modificar esta tarea.");

        if (!Enum.IsDefined(typeof(TaskPriority), request.Priority))
            throw new BadRequestException("Prioridad de tarea inválida.");

        task.Priority = request.Priority;

        await _taskRepository.UpdateAsync(task);

        return ToResponse(task);
    }

    public async Task<TaskResponse> UpdateAsync(
        Guid currentUserId,
        Guid taskId,
        UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        if (task.Project.Status == ProjectStatus.Archived)
            throw new BadRequestException("No se puede modificar una tarea de un proyecto archivado.");

        var isMember = task.Project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para modificar esta tarea.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("El título de la tarea es obligatorio.");

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.DueDate = request.DueDate;

        await _taskRepository.UpdateAsync(task);

        return ToResponse(task);
    }

    public async Task DeleteAsync(Guid currentUserId, Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        if (task.Project.Status == ProjectStatus.Archived)
            throw new BadRequestException("No se puede eliminar una tarea de un proyecto archivado.");

        var isMember = task.Project.Workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para eliminar esta tarea.");

        task.IsActive = false;

        await _taskRepository.UpdateAsync(task);
    }

    public async Task<TaskResponse> AssignAsync(
    Guid currentUserId,
    Guid taskId,
    AssignTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null || !task.IsActive)
            throw new NotFoundException("Tarea no encontrada.");

        if (task.Project.Workspace is null || !task.Project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        if (task.Project.Status == ProjectStatus.Archived)
            throw new BadRequestException("No se puede asignar una tarea de un proyecto archivado.");

        var currentMember = task.Project.Workspace.Members.FirstOrDefault(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (currentMember is null)
            throw new ForbiddenException("No tenés permiso para asignar esta tarea.");

        if (currentMember.Role is not WorkspaceRole.Owner and not WorkspaceRole.Admin)
            throw new ForbiddenException("Solo Owner o Admin pueden asignar tareas.");

        var assignedMember = task.Project.Workspace.Members.FirstOrDefault(member =>
            member.UserId == request.AssignedUserId &&
            member.IsActive);

        if (assignedMember is null)
            throw new BadRequestException("El usuario asignado no pertenece a este workspace.");

        task.AssignedUserId = request.AssignedUserId;

        await _taskRepository.UpdateAsync(task);

        return ToResponse(task);
    }

    private static TaskResponse ToResponse(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            CreatedByUserId = task.CreatedByUserId,
            AssignedUserId = task.AssignedUserId,
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            IsActive = task.IsActive
        };
    }
}