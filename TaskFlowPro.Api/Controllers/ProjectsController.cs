using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowPro.Application.DTOs.Projects;
using TaskFlowPro.Api.Extensions;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost("api/workspaces/{workspaceId:guid}/projects")]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        CreateProjectRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _projectService.CreateAsync(
            currentUserId,
            workspaceId,
            request
        );

        return CreatedAtAction(
            nameof(GetById),
            new { projectId = response.Id },
            response
        );
    }

    [HttpGet("api/workspaces/{workspaceId:guid}/projects")]
    public async Task<IActionResult> GetByWorkspaceId(Guid workspaceId)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _projectService.GetByWorkspaceIdAsync(
            currentUserId,
            workspaceId
        );

        return Ok(response);
    }

    [HttpGet("api/projects/{projectId:guid}")]
    public async Task<IActionResult> GetById(Guid projectId)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _projectService.GetByIdAsync(
            currentUserId,
            projectId
        );

        return Ok(response);
    }
}