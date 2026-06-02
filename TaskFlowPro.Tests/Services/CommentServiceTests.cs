using FluentAssertions;
using Moq;
using TaskFlowPro.Application.DTOs.Comments;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Application.Services;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IWorkspaceAuthorizationService> _workspaceAuthorizationServiceMock;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _workspaceAuthorizationServiceMock = new Mock<IWorkspaceAuthorizationService>();

        _service = new CommentService(
            _commentRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _workspaceAuthorizationServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateComment_WhenUserIsWorkspaceMember()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var request = new CreateCommentRequest
        {
            Content = "Comentario de prueba"
        };

        _taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _commentRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(
            currentUserId,
            taskId,
            request
        );

        // Assert
        result.Should().NotBeNull();
        result.TaskItemId.Should().Be(taskId);
        result.UserId.Should().Be(currentUserId);
        result.Content.Should().Be("Comentario de prueba");
        result.IsActive.Should().BeTrue();

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );

        _commentRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Comment>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var request = new CreateCommentRequest
        {
            Content = "Comentario de prueba"
        };

        _taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var act = async () => await _service.CreateAsync(
            currentUserId,
            taskId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Tarea no encontrada.");

        _commentRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Comment>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBadRequestException_WhenContentIsEmpty()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var request = new CreateCommentRequest
        {
            Content = ""
        };

        _taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var act = async () => await _service.CreateAsync(
            currentUserId,
            taskId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("El contenido del comentario es obligatorio.");

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );

        _commentRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Comment>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetByTaskIdAsync_ShouldReturnActiveComments_WhenUserIsWorkspaceMember()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var comments = new List<Comment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TaskItemId = taskId,
                UserId = currentUserId,
                Content = "Comentario activo 1",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskItemId = taskId,
                UserId = currentUserId,
                Content = "Comentario activo 2",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskItemId = taskId,
                UserId = currentUserId,
                Content = "Comentario inactivo",
                CreatedAt = DateTime.UtcNow,
                IsActive = false
            }
        };

        _taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _commentRepositoryMock
            .Setup(repository => repository.GetByTaskIdAsync(taskId))
            .ReturnsAsync(comments);

        // Act
        var result = await _service.GetByTaskIdAsync(
            currentUserId,
            taskId
        );

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(comment => comment.IsActive);

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateComment_WhenUserIsCommentOwner()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var comment = new Comment
        {
            Id = commentId,
            TaskItemId = taskId,
            TaskItem = task,
            UserId = currentUserId,
            Content = "Comentario original",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        var request = new UpdateCommentRequest
        {
            Content = "Comentario actualizado"
        };

        _commentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        _commentRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(
            currentUserId,
            commentId,
            request
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(commentId);
        result.Content.Should().Be("Comentario actualizado");
        result.UpdatedAt.Should().NotBeNull();

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );

        _commentRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<Comment>(
                item => item.Content == "Comentario actualizado"
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbiddenException_WhenUserIsNotCommentOwner()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var commentOwnerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var comment = new Comment
        {
            Id = commentId,
            TaskItemId = taskId,
            TaskItem = task,
            UserId = commentOwnerId,
            Content = "Comentario ajeno",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var request = new UpdateCommentRequest
        {
            Content = "Intento de edición"
        };

        _commentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act
        var act = async () => await _service.UpdateAsync(
            currentUserId,
            commentId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("Solo podés editar tus propios comentarios.");

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );

        _commentRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Comment>()),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteComment_WhenUserIsCommentOwner()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var comment = new Comment
        {
            Id = commentId,
            TaskItemId = taskId,
            TaskItem = task,
            UserId = currentUserId,
            Content = "Comentario propio",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        _commentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        _commentRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(
            currentUserId,
            commentId
        );

        // Assert
        comment.IsActive.Should().BeFalse();
        comment.UpdatedAt.Should().NotBeNull();

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );

        _commentRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.Is<Comment>(
                item => item.IsActive == false
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbiddenException_WhenUserIsNotCommentOwner()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var commentOwnerId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var task = CreateTaskWithWorkspaceMember(
            taskId,
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var comment = new Comment
        {
            Id = commentId,
            TaskItemId = taskId,
            TaskItem = task,
            UserId = commentOwnerId,
            Content = "Comentario ajeno",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _commentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act
        var act = async () => await _service.DeleteAsync(
            currentUserId,
            commentId
        );

        // Assert
        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("Solo podés eliminar tus propios comentarios.");

        _workspaceAuthorizationServiceMock.Verify(
            service => service.EnsureMember(task.Project.Workspace, currentUserId),
            Times.Once
        );

        _commentRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Comment>()),
            Times.Never
        );
    }

    private static TaskItem CreateTaskWithWorkspaceMember(
        Guid taskId,
        Guid projectId,
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

        var project = new Project
        {
            Id = projectId,
            WorkspaceId = workspaceId,
            Name = "Proyecto de prueba",
            Description = "Proyecto usado en tests",
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Workspace = workspace
        };

        return new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            Project = project,
            Title = "Tarea de prueba",
            Description = "Tarea usada en tests",
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Medium,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}