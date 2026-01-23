using System.Security.Claims;
using archFlowServer.Data;
using archFlowServer.Models.Enums;
using archFlowServer.Utils.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Utils.Authorization.Handlers;

public class CanManageMembersHandler
    : AuthorizationHandler<CanManageMembersRequirement>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CanManageMembersHandler(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanManageMembersRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return;

        // projectId vem da rota
        var routeValues = _httpContextAccessor.HttpContext?.Request.RouteValues;
        if (routeValues == null || !routeValues.TryGetValue("id", out var projectIdObj))
            return;

        if (!Guid.TryParse(projectIdObj?.ToString(), out var projectId))
            return;

        var role = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
            .Select(pm => pm.Role)
            .FirstOrDefaultAsync();

        if (role is MemberRole.Owner or MemberRole.ScrumMaster)
        {
            context.Succeed(requirement);
        }
    }
}
