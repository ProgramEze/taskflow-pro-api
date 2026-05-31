using TaskFlowPro.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowPro.Application.DTOs.Comments;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost("api/tasks/{taskId:guid}/comments")]
    public async Task<IActionResult> Create(
        Guid taskId,
        CreateCommentRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _commentService.CreateAsync(
            currentUserId,
            taskId,
            request
        );

        return CreatedAtAction(
            nameof(GetByTaskId),
            new { taskId = response.TaskItemId },
            response
        );
    }

    [HttpGet("api/tasks/{taskId:guid}/comments")]
    public async Task<IActionResult> GetByTaskId(Guid taskId)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _commentService.GetByTaskIdAsync(
            currentUserId,
            taskId
        );

        return Ok(response);
    }

    [HttpPut("api/comments/{commentId:guid}")]
    public async Task<IActionResult> Update(
        Guid commentId,
        UpdateCommentRequest request)
    {
        var currentUserId = User.GetCurrentUserId();

        var response = await _commentService.UpdateAsync(
            currentUserId,
            commentId,
            request
        );

        return Ok(response);
    }

    [HttpDelete("api/comments/{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid commentId)
    {
        var currentUserId = User.GetCurrentUserId();

        await _commentService.DeleteAsync(
            currentUserId,
            commentId
        );

        return NoContent();
    }
}