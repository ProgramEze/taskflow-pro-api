using System.Net;
using TaskFlowPro.Api.Models;
using TaskFlowPro.Application.Exceptions;

namespace TaskFlowPro.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            ForbiddenException => HttpStatusCode.Forbidden,
            NotFoundException => HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = exception.Message,
            Detail = null,
            Path = context.Request.Path,
            Timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}