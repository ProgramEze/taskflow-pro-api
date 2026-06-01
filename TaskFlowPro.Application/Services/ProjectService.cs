using TaskFlowPro.Application.DTOs.Projects;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAuthorizationService _workspaceAuthorizationService;

    public ProjectService(
        IProjectRepository projectRepository,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceAuthorizationService workspaceAuthorizationService)
    {
        _projectRepository = projectRepository;
        _workspaceRepository = workspaceRepository;
        _workspaceAuthorizationService = workspaceAuthorizationService;
    }

    public async Task<ProjectResponse> CreateAsync(
        Guid currentUserId,
        Guid workspaceId,
        CreateProjectRequest request)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        _workspaceAuthorizationService.EnsureOwnerOrAdmin(
            workspace,
            currentUserId
        );

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("El nombre del proyecto es obligatorio.");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow,
            DueDate = request.DueDate
        };

        await _projectRepository.AddAsync(project);

        return ToResponse(project);
    }

    public async Task<List<ProjectResponse>> GetByWorkspaceIdAsync(
        Guid currentUserId,
        Guid workspaceId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        _workspaceAuthorizationService.EnsureMember(
            workspace,
            currentUserId
        );

        var projects = await _projectRepository.GetByWorkspaceIdAsync(workspaceId);

        return projects
            .Select(ToResponse)
            .ToList();
    }

    public async Task<ProjectResponse> GetByIdAsync(
        Guid currentUserId,
        Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);

        if (project is null)
            throw new NotFoundException("Proyecto no encontrado.");

        if (project.Workspace is null || !project.Workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        _workspaceAuthorizationService.EnsureMember(
            project.Workspace,
            currentUserId
        );

        return ToResponse(project);
    }

    private static ProjectResponse ToResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            WorkspaceId = project.WorkspaceId,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            DueDate = project.DueDate
        };
    }
}