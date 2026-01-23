using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IProductBacklogRepository
{
    Task<ProductBacklog?> GetByProjectIdAsync(Guid projectId);
    Task SaveChangesAsync();
}
