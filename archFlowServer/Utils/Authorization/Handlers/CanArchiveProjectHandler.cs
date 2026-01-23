using System.Security.Claims;
using archFlowServer.Data;
using archFlowServer.Models.Enums;
using archFlowServer.Utils.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Utils.Authorization.Handlers;

public class CanArchiveProjectHandler : AuthorizationHandler<CanArchiveProjectRequirement>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _http;

    public CanArchiveProjectHandler(AppDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanArchiveProjectRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return;

        var routeValues = _http.HttpContext?.Request.RouteValues;
        if (routeValues == null || !routeValues.TryGetValue("id", out var projectIdObj))
            return;

        if (!Guid.TryParse(projectIdObj?.ToString(), out var projectId))
            return;

        var role = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
            .Select(pm => pm.Role)
            .FirstOrDefaultAsync();

        if (role == MemberRole.Owner)
            context.Succeed(requirement);
    }
}
