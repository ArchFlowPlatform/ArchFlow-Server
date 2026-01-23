using archFlowServer.Models.Exceptions;
using archFlowServer.Models.ViewModels;

namespace archFlowServer.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                ResultViewModel.Fail(ex.Message, ex.Errors)
            );
        }
        catch (ConflictException ex)
        {
            await WriteAsync(
                context,
                StatusCodes.Status409Conflict,
                ResultViewModel.Fail(ex.Message)
            );
        }
        catch (ForbiddenException ex)
        {
            await WriteAsync(
                context,
                StatusCodes.Status403Forbidden,
                ResultViewModel.Fail(ex.Message)
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(
                context,
                StatusCodes.Status401Unauthorized,
                ResultViewModel.Fail(ex.Message)
            );
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(
                context,
                StatusCodes.Status404NotFound,
                ResultViewModel.Fail(ex.Message)
            );
        }
        catch (BadRequestException ex)
        {
            await WriteAsync(
                context,
                StatusCodes.Status400BadRequest,
                ResultViewModel.Fail(ex.Message)
            );
        }
        catch (DomainException ex)
        {
            // fallback para regras de negÃ³cio genÃ©ricas que vocÃª não categorizou ainda
            await WriteAsync(
                context,
                StatusCodes.Status400BadRequest,
                ResultViewModel.Fail(ex.Message)
            );
        }
        catch (Exception)
        {
            await WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ResultViewModel.Fail("Ocorreu um erro interno no servidor.")
            );
        }
    }

    private static Task WriteAsync(
        HttpContext context,
        int statusCode,
        ResultViewModel result)
    {
        // Se algo jÃ¡ escreveu no response, não tenta reescrever
        if (context.Response.HasStarted)
            return Task.CompletedTask;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(result);
    }
}

