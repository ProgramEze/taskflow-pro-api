using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowPro.Application.DTOs.Workspaces;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Api.Extensions;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _workspaceService.CreateAsync(currentUserId, request);

        return CreatedAtAction(nameof(GetById), new { workspaceId = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWorkspaces()
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _workspaceService.GetMyWorkspacesAsync(currentUserId);

        return Ok(response);
    }

    [HttpGet("{workspaceId:guid}")]
    public async Task<IActionResult> GetById(Guid workspaceId)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _workspaceService.GetByIdAsync(currentUserId, workspaceId);

        return Ok(response);
    }
}