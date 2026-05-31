using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowPro.Api.Extensions;
using TaskFlowPro.Application.DTOs.Members;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Authorize]
public class WorkspaceMembersController : ControllerBase
{
    private readonly IWorkspaceMemberService _memberService;

    public WorkspaceMembersController(IWorkspaceMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpPost("api/workspaces/{workspaceId:guid}/members")]
    public async Task<IActionResult> AddMember(
        Guid workspaceId,
        AddWorkspaceMemberRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _memberService.AddMemberAsync(
            currentUserId,
            workspaceId,
            request
        );

        return CreatedAtAction(
            nameof(GetMembers),
            new { workspaceId },
            response
        );
    }

    [HttpGet("api/workspaces/{workspaceId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid workspaceId)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _memberService.GetMembersAsync(
            currentUserId,
            workspaceId
        );

        return Ok(response);
    }

    [HttpPut("api/workspaces/{workspaceId:guid}/members/{memberId:guid}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid workspaceId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _memberService.ChangeRoleAsync(
            currentUserId,
            workspaceId,
            memberId,
            request
        );

        return Ok(response);
    }

    [HttpDelete("api/workspaces/{workspaceId:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid workspaceId,
        Guid memberId)
    {
        var currentUserId = User.GetCurrentUserId();

        await _memberService.RemoveMemberAsync(
            currentUserId,
            workspaceId,
            memberId
        );

        return NoContent();
    }
}