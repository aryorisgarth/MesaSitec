using System.Text.Json;
using MesaSitec.Application.Exceptions;

namespace MesaSitec.Api.Middleware;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteProblemAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            var detail = environment.IsDevelopment()
                ? ex.Message
                : "Ocurrió un error inesperado.";
            await WriteProblemAsync(context, new AppException(
                500,
                "ERROR_INTERNO",
                "Error interno",
                detail));
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, AppException ex)
    {
        context.Response.StatusCode = ex.StatusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new Dictionary<string, object?>
        {
            ["type"] = ex.TypeUri,
            ["title"] = ex.Title,
            ["status"] = ex.StatusCode,
            ["detail"] = ex.Message,
            ["codigo"] = ex.Codigo,
        };

        if (ex.Errores is not null)
            problem["errores"] = ex.Errores;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
