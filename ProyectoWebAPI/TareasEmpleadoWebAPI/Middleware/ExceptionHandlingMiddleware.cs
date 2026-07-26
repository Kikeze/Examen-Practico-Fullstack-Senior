using System.Net;
using System.Text.Json;

using TareasEmpleadoWebAPI.DTOs;

namespace TareasEmpleadoWebAPI.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _environment = environment;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "Ocurrio un error al procesar la solicitud.");

        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new ErrorResponseDto
        {
            StatusCode = (int)statusCode,
            Message = statusCode == HttpStatusCode.InternalServerError
                ? "Ocurrio un error interno en el servidor."
                : exception.Message,
            Detail = _environment.IsDevelopment()
                ? $"{exception.Message} | InnerException: {exception.InnerException?.Message}"
                : null
        };

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }
}
