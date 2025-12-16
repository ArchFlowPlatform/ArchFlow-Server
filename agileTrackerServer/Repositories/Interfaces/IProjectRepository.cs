using agileTrackerServer.Models.Entities;

namespace agileTrackerServer.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(Guid id, Guid OwnerId);
        Task AddAsync(Project project);
        Task SaveChangesAsync();
    }
}