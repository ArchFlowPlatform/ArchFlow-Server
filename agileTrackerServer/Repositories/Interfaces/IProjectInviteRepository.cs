using agileTrackerServer.Models.Entities;

namespace agileTrackerServer.Repositories.Interfaces;

public interface IProjectInviteRepository
{
    Task AddAsync(ProjectInvite invite);
    Task<ProjectInvite?> GetByTokenAsync(string token);
    Task<bool> ExistsActiveInviteAsync(Guid projectId, string email);
    void Delete(ProjectInvite invite);
    Task SaveChangesAsync();
}
