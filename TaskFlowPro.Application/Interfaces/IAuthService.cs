using TaskFlowPro.Application.DTOs.Auth;

namespace TaskFlowPro.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> GetCurrentUserAsync(Guid userId);
}