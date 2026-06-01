using TaskFlowPro.Application.DTOs.Members;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.Services;

public class WorkspaceMemberService : IWorkspaceMemberService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceMemberRepository _memberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceAuthorizationService _workspaceAuthorizationService;

    public WorkspaceMemberService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMemberRepository memberRepository,
        IUserRepository userRepository,
        IWorkspaceAuthorizationService workspaceAuthorizationService)
    {
        _workspaceRepository = workspaceRepository;
        _memberRepository = memberRepository;
        _userRepository = userRepository;
        _workspaceAuthorizationService = workspaceAuthorizationService;
    }

    public async Task<WorkspaceMemberResponse> AddMemberAsync(
        Guid currentUserId,
        Guid workspaceId,
        AddWorkspaceMemberRequest request)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        _workspaceAuthorizationService.EnsureOwnerOrAdmin(
            workspace,
            currentUserId
        );

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("El email del usuario es obligatorio.");

        var email = request.Email.Trim().ToLower();

        var userToAdd = await _userRepository.GetByEmailAsync(email);

        if (userToAdd is null || !userToAdd.IsActive)
            throw new NotFoundException("Usuario no encontrado.");

        var existingMember = await _memberRepository.GetByWorkspaceAndUserIdAsync(
            workspaceId,
            userToAdd.Id
        );

        if (existingMember is not null && existingMember.IsActive)
            throw new ConflictException("El usuario ya pertenece a este workspace.");

        if (existingMember is not null && !existingMember.IsActive)
        {
            existingMember.IsActive = true;
            existingMember.Role = WorkspaceRole.Member;
            existingMember.JoinedAt = DateTime.UtcNow;

            await _memberRepository.UpdateAsync(existingMember);

            var reactivatedMember = await _memberRepository.GetByIdAsync(existingMember.Id);

            if (reactivatedMember is null)
                throw new NotFoundException("Miembro no encontrado.");

            return ToResponse(reactivatedMember);
        }

        var newMember = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = userToAdd.Id,
            Role = WorkspaceRole.Member,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _memberRepository.AddAsync(newMember);

        var createdMember = await _memberRepository.GetByIdAsync(newMember.Id);

        if (createdMember is null)
            throw new NotFoundException("Miembro no encontrado.");

        return ToResponse(createdMember);
    }

    public async Task<List<WorkspaceMemberResponse>> GetMembersAsync(
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

        var members = await _memberRepository.GetByWorkspaceIdAsync(workspaceId);

        return members
            .Where(member => member.IsActive)
            .Select(ToResponse)
            .ToList();
    }

    public async Task<WorkspaceMemberResponse> ChangeRoleAsync(
        Guid currentUserId,
        Guid workspaceId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        _workspaceAuthorizationService.EnsureOwner(
            workspace,
            currentUserId
        );

        if (!Enum.IsDefined(typeof(WorkspaceRole), request.Role))
            throw new BadRequestException("Rol inválido.");

        var memberToUpdate = await _memberRepository.GetByIdAsync(memberId);

        if (memberToUpdate is null || !memberToUpdate.IsActive)
            throw new NotFoundException("Miembro no encontrado.");

        if (memberToUpdate.WorkspaceId != workspaceId)
            throw new BadRequestException("El miembro no pertenece a este workspace.");

        if (memberToUpdate.Role == WorkspaceRole.Owner)
            throw new BadRequestException("No se puede modificar el rol del Owner.");

        if (request.Role == WorkspaceRole.Owner)
            throw new BadRequestException("No se puede asignar el rol Owner desde este endpoint.");

        memberToUpdate.Role = request.Role;

        await _memberRepository.UpdateAsync(memberToUpdate);

        var updatedMember = await _memberRepository.GetByIdAsync(memberToUpdate.Id);

        if (updatedMember is null)
            throw new NotFoundException("Miembro no encontrado.");

        return ToResponse(updatedMember);
    }

    public async Task RemoveMemberAsync(
        Guid currentUserId,
        Guid workspaceId,
        Guid memberId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace no encontrado.");

        var currentMember = _workspaceAuthorizationService.GetCurrentMemberOrThrow(
            workspace,
            currentUserId
        );

        if (currentMember.Role is not WorkspaceRole.Owner and not WorkspaceRole.Admin)
            throw new ForbiddenException("Solo Owner o Admin pueden realizar esta acción.");

        var memberToRemove = await _memberRepository.GetByIdAsync(memberId);

        if (memberToRemove is null || !memberToRemove.IsActive)
            throw new NotFoundException("Miembro no encontrado.");

        if (memberToRemove.WorkspaceId != workspaceId)
            throw new BadRequestException("El miembro no pertenece a este workspace.");

        if (memberToRemove.Role == WorkspaceRole.Owner)
            throw new BadRequestException("No se puede quitar al Owner del workspace.");

        if (currentMember.Role == WorkspaceRole.Admin &&
            memberToRemove.Role == WorkspaceRole.Admin)
            throw new ForbiddenException("Un Admin no puede quitar a otro Admin.");

        memberToRemove.IsActive = false;

        await _memberRepository.UpdateAsync(memberToRemove);
    }

    private static WorkspaceMemberResponse ToResponse(WorkspaceMember member)
    {
        return new WorkspaceMemberResponse
        {
            Id = member.Id,
            WorkspaceId = member.WorkspaceId,
            UserId = member.UserId,
            UserFullName = $"{member.User.FirstName} {member.User.LastName}",
            Email = member.User.Email,
            Role = member.Role,
            JoinedAt = member.JoinedAt,
            IsActive = member.IsActive
        };
    }
}