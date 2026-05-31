using TaskFlowPro.Application.DTOs.Workspaces;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
    }

    public async Task<WorkspaceResponse> CreateAsync(Guid currentUserId, CreateWorkspaceRequest request)
    {
        var user = await _userRepository.GetByIdAsync(currentUserId);

        if (user is null)
            throw new NotFoundException("Usuario no encontrado.");

        if (!user.IsActive)
            throw new UnauthorizedException("El usuario se encuentra desactivado.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("El nombre del workspace es obligatorio.");

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            OwnerId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var ownerMember = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            UserId = currentUserId,
            Role = WorkspaceRole.Owner,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        workspace.Members.Add(ownerMember);

        await _workspaceRepository.AddAsync(workspace);

        return ToResponse(workspace);
    }

    public async Task<List<WorkspaceResponse>> GetMyWorkspacesAsync(Guid currentUserId)
    {
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);

        return workspaces
            .Select(ToResponse)
            .ToList();
    }

    public async Task<WorkspaceResponse> GetByIdAsync(Guid currentUserId, Guid workspaceId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        var isMember = workspace.Members.Any(member =>
            member.UserId == currentUserId &&
            member.IsActive);

        if (!isMember)
            throw new ForbiddenException("No tenés permiso para acceder a este workspace.");

        return ToResponse(workspace);
    }

    private static WorkspaceResponse ToResponse(Workspace workspace)
    {
        return new WorkspaceResponse
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            OwnerId = workspace.OwnerId,
            CreatedAt = workspace.CreatedAt
        };
    }
}