using archFlowServer.Models.Enums;

namespace ArchFlowServer.Models.Dtos.Project
{
    public sealed record ProjectMemberDto(
    int Id,
    Guid ProjectId,
    Guid UserId,
    MemberRole Role,
    DateTime JoinedAt
);
}
