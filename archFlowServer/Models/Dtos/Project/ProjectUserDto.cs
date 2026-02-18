using archFlowServer.Models.Enums;

namespace ArchFlowServer.Models.Dtos.Project
{
    public sealed record ProjectUserDto(
    Guid UserId,
    string Name,
    string Email,
    string AvatarUrl,
    MemberRole Role,
    DateTime JoinedAt
);
}
