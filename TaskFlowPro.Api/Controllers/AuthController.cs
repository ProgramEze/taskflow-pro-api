using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlowPro.Api.Extensions;
using TaskFlowPro.Application.DTOs.Auth;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);

        return Created(string.Empty, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetCurrentUserId();

        var response = await _authService.GetCurrentUserAsync(userId);

        return Ok(response);
    }
}