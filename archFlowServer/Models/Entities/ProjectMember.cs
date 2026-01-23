using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class ProjectMember
{
    public int Id { get; private set; }

    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }

    public MemberRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public Project Project { get; private set; } = null!;
    public User User { get; private set; } = null!;
    private ProjectMember() { }

    internal ProjectMember(Guid projectId, Guid userId, MemberRole role)
    {
        ProjectId = projectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }
}


