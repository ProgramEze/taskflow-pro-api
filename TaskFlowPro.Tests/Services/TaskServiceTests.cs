using FluentAssertions;
using Moq;
using TaskFlowPro.Application.DTOs.Common;
using TaskFlowPro.Application.DTOs.Tasks;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Application.Services;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _projectRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTask_WhenRequestIsValid()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = CreateProjectWithMember(
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Owner
        );

        var request = new CreateTaskRequest
        {
            Title = "Crear endpoints de tareas",
            Description = "Implementar creación de tareas",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _taskRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _taskService.CreateAsync(
            currentUserId,
            projectId,
            request
        );

        // Assert
        result.Should().NotBeNull();
        result.ProjectId.Should().Be(projectId);
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Priority.Should().Be(TaskPriority.High);
        result.Status.Should().Be(TaskItemStatus.Pending);
        result.CreatedByUserId.Should().Be(currentUserId);
        result.IsActive.Should().BeTrue();

        _taskRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<TaskItem>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var request = new CreateTaskRequest
        {
            Title = "Crear tarea",
            Priority = TaskPriority.Medium
        };

        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act
        var act = async () => await _taskService.CreateAsync(
            currentUserId,
            projectId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Proyecto no encontrado.");

        _taskRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<TaskItem>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForbiddenException_WhenUserIsNotWorkspaceMember()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = CreateProjectWithMember(
            projectId,
            workspaceId,
            anotherUserId,
            WorkspaceRole.Member
        );

        var request = new CreateTaskRequest
        {
            Title = "Crear tarea",
            Priority = TaskPriority.Medium
        };

        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var act = async () => await _taskService.CreateAsync(
            currentUserId,
            projectId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("No tenés permiso para crear tareas en este proyecto.");

        _taskRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<TaskItem>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBadRequestException_WhenTitleIsEmpty()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = CreateProjectWithMember(
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Owner
        );

        var request = new CreateTaskRequest
        {
            Title = "",
            Priority = TaskPriority.Medium
        };

        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var act = async () => await _taskService.CreateAsync(
            currentUserId,
            projectId,
            request
        );

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("El título de la tarea es obligatorio.");

        _taskRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<TaskItem>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldReturnPagedTasks_WhenUserIsWorkspaceMember()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = CreateProjectWithMember(
            projectId,
            workspaceId,
            currentUserId,
            WorkspaceRole.Member
        );

        var query = new TaskQueryParameters
        {
            PageNumber = 1,
            PageSize = 2
        };

        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = "Tarea 1",
                Description = "Descripción 1",
                Status = TaskItemStatus.Pending,
                Priority = TaskPriority.High,
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = "Tarea 2",
                Description = "Descripción 2",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Medium,
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        var pagedResult = new PagedResult<TaskItem>
        {
            Items = tasks,
            TotalItems = 5
        };

        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _taskRepositoryMock
            .Setup(repository => repository.GetByProjectIdPagedAsync(projectId, query))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _taskService.GetByProjectIdAsync(
            currentUserId,
            projectId,
            query
        );

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalItems.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    private static Project CreateProjectWithMember(
        Guid projectId,
        Guid workspaceId,
        Guid userId,
        WorkspaceRole role)
    {
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Workspace de prueba",
            Description = "Workspace usado en tests",
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var member = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            Workspace = workspace
        };

        workspace.Members.Add(member);

        return new Project
        {
            Id = projectId,
            WorkspaceId = workspaceId,
            Name = "Proyecto de prueba",
            Description = "Proyecto usado en tests",
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Workspace = workspace
        };
    }
}