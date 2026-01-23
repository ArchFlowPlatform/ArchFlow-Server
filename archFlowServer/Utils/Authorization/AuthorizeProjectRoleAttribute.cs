using archFlowServer.Models.Enums;

namespace archFlowServer.Utils.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorizeProjectRoleAttribute : Attribute
{
    public MemberRole[] Roles { get; }

    public AuthorizeProjectRoleAttribute(params MemberRole[] roles)
    {
        Roles = roles;
    }
}
