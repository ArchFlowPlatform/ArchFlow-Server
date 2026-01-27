using System.Security.Claims;
using archFlowServer.Data;
using archFlowServer.Models.Enums;
using archFlowServer.Utils.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Utils.Authorization.Handlers;

public class CanManageMembersHandler : AuthorizationHandler<CanManageMembersRequirement>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CanManageMembersHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
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

        var http = _httpContextAccessor.HttpContext;
        var routeValues = http?.Request.RouteValues;
        if (routeValues == null) return;

        Guid projectId;

        // 1) Tenta pegar projectId direto da rota (id OU projectId)
        if (TryGetGuidRouteValue(routeValues, "id", out projectId) ||
            TryGetGuidRouteValue(routeValues, "projectId", out projectId))
        {
            // ok
        }
        else
        {
            // 2) Sem projectId na rota: tenta via token do invite
            if (!routeValues.TryGetValue("token", out var tokenObj))
                return;

            var token = tokenObj?.ToString();
            if (string.IsNullOrWhiteSpace(token))
                return;

            projectId = await _context.ProjectInvites
                .Where(i => i.Token == token)
                .Select(i => i.ProjectId)
                .FirstOrDefaultAsync();

            if (projectId == Guid.Empty)
                return;
        }

        var role = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
            .Select(pm => pm.Role)
            .FirstOrDefaultAsync();

        if (role is MemberRole.Owner or MemberRole.ScrumMaster)
            context.Succeed(requirement);
    }

    private static bool TryGetGuidRouteValue(
        Microsoft.AspNetCore.Routing.RouteValueDictionary values,
        string key,
        out Guid guid)
    {
        guid = Guid.Empty;
        if (!values.TryGetValue(key, out var obj)) return false;
        return Guid.TryParse(obj?.ToString(), out guid);
    }
}
