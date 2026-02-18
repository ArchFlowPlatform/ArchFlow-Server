using archFlowServer.Models.Enums;

namespace ArchFlowServer.Models.Dtos.Project
{
    public sealed record ProjectListDto(
    Guid Id,
    string Name,
    string Description,
    ProjectStatus Status,
    DateTime CreatedAt,
    IReadOnlyCollection<ProjectUserDto> Members
);
}
