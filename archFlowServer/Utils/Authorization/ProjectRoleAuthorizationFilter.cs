using archFlowServer.Services;
using archFlowServer.Utils.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace archFlowServer.Utils.Authorization;

public class ProjectRoleAuthorizationFilter : IAsyncActionFilter
{
    private readonly ProjectAuthorizationService _authService;

    public ProjectRoleAuthorizationFilter(ProjectAuthorizationService authService)
    {
        _authService = authService;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var attribute = context.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeProjectRoleAttribute>()
            .FirstOrDefault();

        if (attribute is null)
        {
            await next();
            return;
        }

        if (!context.ActionArguments.TryGetValue("id", out var projectIdObj) ||
            projectIdObj is not Guid projectId)
        {
            context.Result = new BadRequestObjectResult("ProjectId inválido.");
            return;
        }

        var userId = context.HttpContext.User.GetUserId();

        await _authService.AuthorizeAsync(
            projectId,
            userId,
            attribute.Roles
        );

        await next();
    }
}
