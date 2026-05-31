namespace TaskFlowPro.Application.DTOs.Auth;

public class AuthResponse
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}