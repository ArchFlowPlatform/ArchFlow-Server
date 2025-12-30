using agileTrackerServer.Models.Enums;
using agileTrackerServer.Models.Exceptions;

namespace agileTrackerServer.Models.Entities;

public class ProjectMember
{
    public int Id {get; private set;}
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public MemberRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    
    private ProjectMember () { }

    public ProjectMember(Guid projectId, Guid userId, MemberRole role)
    {
        if (projectId == Guid.Empty)
            throw new DomainException("ProjectId inválido.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId inválido.");

        ProjectId = projectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }
}

