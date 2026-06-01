using FluentAssertions;
using Moq;
using TaskFlowPro.Application.DTOs.Members;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Application.Services;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Tests.Services;

public class WorkspaceMemberServiceTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<IWorkspaceMemberRepository> _memberRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly WorkspaceMemberService _service;

    public WorkspaceMemberServiceTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _memberRepositoryMock = new Mock<IWorkspaceMemberRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _service = new WorkspaceMemberService(
            _workspaceRepositoryMock.Object,
            _memberRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    [Fact]
    public async Task AddMemberAsync_ShouldAddMember_WhenCurrentUserIsOwner()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userToAddId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            ownerId,
            WorkspaceRole.Owner
        );

        var userToAdd = new User
        {
            Id = userToAddId,
            FirstName = "Ana",
            LastName = "Gomez",
            Email = "ana@test.com",
            IsActive = true
        };

        var request = new AddWorkspaceMemberRequest
        {
            Email = "ana@test.com"
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync("ana@test.com"))
            .ReturnsAsync(userToAdd);

        _memberRepositoryMock
            .Setup(repository => repository.GetByWorkspaceAndUserIdAsync(workspaceId, userToAddId))
            .ReturnsAsync((WorkspaceMember?)null);

        _memberRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<WorkspaceMember>()))
            .Returns(Task.CompletedTask);

        _memberRepositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid memberId) => new WorkspaceMember
            {
                Id = memberId,
                WorkspaceId = workspaceId,
                UserId = userToAddId,
                User = userToAdd,
                Role = WorkspaceRole.Member,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });

        // Act
        var result = await _service.AddMemberAsync(
            ownerId,
            workspaceId,
            request
        );

        // Assert
        result.Should().NotBeNull();
        result.WorkspaceId.Should().Be(workspaceId);
        result.UserId.Should().Be(userToAddId);
        result.Email.Should().Be("ana@test.com");
        result.Role.Should().Be(WorkspaceRole.Member);
        result.IsActive.Should().BeTrue();

        _memberRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<WorkspaceMember>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AddMemberAsync_ShouldThrowForbiddenException_WhenCurrentUserIsMember()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            memberId,
            WorkspaceRole.Member
        );

        var request = new AddWorkspaceMemberRequest
        {
            Email = "ana@test.com"
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        // Act
        var act = async () => await _service.AddMemberAsync(
            memberId,
            workspaceId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("No tenés permiso para agregar miembros.");

        _memberRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<WorkspaceMember>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AddMemberAsync_ShouldThrowConflictException_WhenUserIsAlreadyActiveMember()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userToAddId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            ownerId,
            WorkspaceRole.Owner
        );

        var userToAdd = new User
        {
            Id = userToAddId,
            FirstName = "Ana",
            LastName = "Gomez",
            Email = "ana@test.com",
            IsActive = true
        };

        var existingMember = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = userToAddId,
            User = userToAdd,
            Role = WorkspaceRole.Member,
            IsActive = true
        };

        var request = new AddWorkspaceMemberRequest
        {
            Email = "ana@test.com"
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync("ana@test.com"))
            .ReturnsAsync(userToAdd);

        _memberRepositoryMock
            .Setup(repository => repository.GetByWorkspaceAndUserIdAsync(workspaceId, userToAddId))
            .ReturnsAsync(existingMember);

        // Act
        var act = async () => await _service.AddMemberAsync(
            ownerId,
            workspaceId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("El usuario ya pertenece a este workspace.");

        _memberRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<WorkspaceMember>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldChangeRole_WhenCurrentUserIsOwner()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            ownerId,
            WorkspaceRole.Owner
        );

        var memberToUpdate = new WorkspaceMember
        {
            Id = memberId,
            WorkspaceId = workspaceId,
            UserId = targetUserId,
            User = new User
            {
                Id = targetUserId,
                FirstName = "Ana",
                LastName = "Gomez",
                Email = "ana@test.com"
            },
            Role = WorkspaceRole.Member,
            IsActive = true
        };

        var request = new UpdateWorkspaceMemberRoleRequest
        {
            Role = WorkspaceRole.Admin
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        _memberRepositoryMock
            .Setup(repository => repository.GetByIdAsync(memberId))
            .ReturnsAsync(memberToUpdate);

        _memberRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<WorkspaceMember>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ChangeRoleAsync(
            ownerId,
            workspaceId,
            memberId,
            request
        );

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(WorkspaceRole.Admin);

        _memberRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<WorkspaceMember>(
                member => member.Role == WorkspaceRole.Admin
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldThrowForbiddenException_WhenCurrentUserIsAdmin()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            adminId,
            WorkspaceRole.Admin
        );

        var request = new UpdateWorkspaceMemberRoleRequest
        {
            Role = WorkspaceRole.Admin
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        // Act
        var act = async () => await _service.ChangeRoleAsync(
            adminId,
            workspaceId,
            memberId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("Solo el Owner puede cambiar roles.");

        _memberRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<WorkspaceMember>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldThrowBadRequestException_WhenTryingToModifyOwnerRole()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var ownerMemberId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            ownerId,
            WorkspaceRole.Owner
        );

        var ownerMember = new WorkspaceMember
        {
            Id = ownerMemberId,
            WorkspaceId = workspaceId,
            UserId = ownerId,
            User = new User
            {
                Id = ownerId,
                FirstName = "Owner",
                LastName = "User",
                Email = "owner@test.com"
            },
            Role = WorkspaceRole.Owner,
            IsActive = true
        };

        var request = new UpdateWorkspaceMemberRoleRequest
        {
            Role = WorkspaceRole.Admin
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        _memberRepositoryMock
            .Setup(repository => repository.GetByIdAsync(ownerMemberId))
            .ReturnsAsync(ownerMember);

        // Act
        var act = async () => await _service.ChangeRoleAsync(
            ownerId,
            workspaceId,
            ownerMemberId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("No se puede modificar el rol del Owner.");

        _memberRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<WorkspaceMember>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldRemoveMember_WhenCurrentUserIsOwner()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            ownerId,
            WorkspaceRole.Owner
        );

        var memberToRemove = new WorkspaceMember
        {
            Id = memberId,
            WorkspaceId = workspaceId,
            UserId = targetUserId,
            User = new User
            {
                Id = targetUserId,
                FirstName = "Ana",
                LastName = "Gomez",
                Email = "ana@test.com"
            },
            Role = WorkspaceRole.Member,
            IsActive = true
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        _memberRepositoryMock
            .Setup(repository => repository.GetByIdAsync(memberId))
            .ReturnsAsync(memberToRemove);

        _memberRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<WorkspaceMember>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveMemberAsync(
            ownerId,
            workspaceId,
            memberId
        );

        // Assert
        memberToRemove.IsActive.Should().BeFalse();

        _memberRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<WorkspaceMember>(
                member => member.IsActive == false
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldThrowBadRequestException_WhenTryingToRemoveOwner()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var ownerMemberId = Guid.NewGuid();

        var workspace = CreateWorkspaceWithMember(
            workspaceId,
            ownerId,
            WorkspaceRole.Owner
        );

        var ownerMember = new WorkspaceMember
        {
            Id = ownerMemberId,
            WorkspaceId = workspaceId,
            UserId = ownerId,
            User = new User
            {
                Id = ownerId,
                FirstName = "Owner",
                LastName = "User",
                Email = "owner@test.com"
            },
            Role = WorkspaceRole.Owner,
            IsActive = true
        };

        _workspaceRepositoryMock
            .Setup(repository => repository.GetByIdAsync(workspaceId))
            .ReturnsAsync(workspace);

        _memberRepositoryMock
            .Setup(repository => repository.GetByIdAsync(ownerMemberId))
            .ReturnsAsync(ownerMember);

        // Act
        var act = async () => await _service.RemoveMemberAsync(
            ownerId,
            workspaceId,
            ownerMemberId
        );

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("No se puede quitar al Owner del workspace.");

        _memberRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<WorkspaceMember>()),
            Times.Never
        );
    }

    private static Workspace CreateWorkspaceWithMember(
        Guid workspaceId,
        Guid userId,
        WorkspaceRole role)
    {
        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            IsActive = true
        };

        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Workspace de prueba",
            Description = "Workspace usado en tests",
            OwnerId = role == WorkspaceRole.Owner ? userId : Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var member = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = userId,
            User = user,
            Workspace = workspace,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        workspace.Members.Add(member);

        return workspace;
    }
}