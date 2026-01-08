using agileTrackerServer.Models.Entities;

namespace agileTrackerServer.Repositories.Interfaces;

public interface IProductBacklogRepository
{
    Task<ProductBacklog?> GetByProjectIdAsync(Guid projectId);
    Task SaveChangesAsync();
}