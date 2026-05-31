namespace TaskFlowPro.Api.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public string Path { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}