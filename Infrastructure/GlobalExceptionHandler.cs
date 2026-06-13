using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Sales.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var status = StatusCodes.Status500InternalServerError;
        var title = "Error interno del servidor";
        var detail = "Ocurrio un error inesperado. Intenta nuevamente o contacta a soporte.";

        if (exception is KeyNotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            title = "Recurso no encontrado";
            detail = exception.Message;
        }
        else if (exception is ArgumentException || exception is ArgumentNullException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Argumento invalido";
            detail = exception.Message;
        }
        else if (exception is InvalidOperationException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Operacion invalida";
            detail = exception.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            status = StatusCodes.Status403Forbidden;
            title = "Acceso no autorizado";
            detail = exception.Message;
        }
        else if (exception is HttpRequestException || exception.GetType().Name == "BrokenCircuitException")
        {
            status = StatusCodes.Status503ServiceUnavailable;
            title = "Servicio no disponible";
            detail = "El servicio de inventario no esta disponible temporalmente. Intente nuevamente en unos momentos.";
        }

        var traceId = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        var responsePayload = new
        {
            status = status,
            title = title,
            detail = detail,
            instance = httpContext.Request.Path.Value,
            traceId = traceId
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(responsePayload, jsonOptions);

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }
}
