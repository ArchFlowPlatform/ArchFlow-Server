using System.Security.Claims;
using archFlowServer.Data;
using archFlowServer.Utils.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Utils.Authorization.Handlers;

public class CanViewProjectHandler : AuthorizationHandler<CanViewProjectRequirement>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _http;

    public CanViewProjectHandler(AppDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanViewProjectRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return;

        var routeValues = _http.HttpContext?.Request.RouteValues;
        if (routeValues == null)
            return;

        // aceita tanto "projectId" quanto "id"
        object? projectIdObj = null;

        if (!routeValues.TryGetValue("projectId", out projectIdObj) &&
            !routeValues.TryGetValue("id", out projectIdObj))
            return;

        if (!Guid.TryParse(projectIdObj?.ToString(), out var projectId))
            return;

        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

        if (isMember)
            context.Succeed(requirement);
    }

}
