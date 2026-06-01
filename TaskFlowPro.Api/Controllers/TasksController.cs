using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowPro.Application.DTOs.Tasks;
using TaskFlowPro.Api.Extensions;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> Create(
        Guid projectId,
        CreateTaskRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.CreateAsync(
            currentUserId,
            projectId,
            request
        );

        return CreatedAtAction(
            nameof(GetById),
            new { taskId = response.Id },
            response
        );
    }

    [HttpGet("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> GetByProjectId(
    Guid projectId,
    [FromQuery] TaskQueryParameters query)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.GetByProjectIdAsync(
            currentUserId,
            projectId,
            query
        );

        return Ok(response);
    }

    [HttpGet("api/tasks/{taskId:guid}")]
    public async Task<IActionResult> GetById(Guid taskId)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.GetByIdAsync(
            currentUserId,
            taskId
        );

        return Ok(response);
    }

    [HttpPatch("api/tasks/{taskId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid taskId,
        UpdateTaskStatusRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.ChangeStatusAsync(
            currentUserId,
            taskId,
            request
        );

        return Ok(response);
    }

    [HttpPatch("api/tasks/{taskId:guid}/priority")]
    public async Task<IActionResult> ChangePriority(
        Guid taskId,
        UpdateTaskPriorityRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.ChangePriorityAsync(
            currentUserId,
            taskId,
            request
        );

        return Ok(response);
    }

    [HttpPut("api/tasks/{taskId:guid}")]
    public async Task<IActionResult> Update(
        Guid taskId,
        UpdateTaskRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.UpdateAsync(
            currentUserId,
            taskId,
            request
        );

        return Ok(response);
    }

    [HttpPatch("api/tasks/{taskId:guid}/assign")]
    public async Task<IActionResult> Assign(
    Guid taskId,
    AssignTaskRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _taskService.AssignAsync(
            currentUserId,
            taskId,
            request
        );

        return Ok(response);
    }

    [HttpDelete("api/tasks/{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId)
    {
        var currentUserId = User.GetCurrentUserId();

        await _taskService.DeleteAsync(currentUserId, taskId);

        return NoContent();
    }
}