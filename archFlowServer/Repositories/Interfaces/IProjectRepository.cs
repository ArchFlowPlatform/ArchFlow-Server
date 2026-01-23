using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync(Guid ownerId);
        Task<Project?> GetByIdAsync(Guid id, Guid ownerId);
        Task AddAsync(Project project);
        Task<IEnumerable<ProjectMember>> GetMembersAsync(Guid projectId, Guid userId);
        Task SaveChangesAsync();
    }
}
