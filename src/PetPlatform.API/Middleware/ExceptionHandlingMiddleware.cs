using System.Net;
using System.Text.Json;
using FluentValidation;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        var (statusCode, errorCode, message) = exception switch
        {
            UnauthorizedPostAccessException ex => (HttpStatusCode.Forbidden, ex.Code, ex.Message),
            DomainException domainEx => (HttpStatusCode.BadRequest, domainEx.Code, domainEx.Message),
            ValidationException validationEx => (HttpStatusCode.BadRequest, "VALIDATION_ERROR",
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", "Resurs nije pronađen."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Niste autorizovani."),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "Došlo je do greške na serveru.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = new
            {
                code = errorCode,
                message,
                details = new { }
            }
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
