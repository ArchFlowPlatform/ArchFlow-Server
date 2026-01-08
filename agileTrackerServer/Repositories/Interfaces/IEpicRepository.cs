using agileTrackerServer.Models.Entities;

namespace agileTrackerServer.Repositories.Interfaces;

public interface IEpicRepository
{
    Task<Epic?> GetByIdAsync(Guid epicId);
    Task AddAsync(Epic epic);
    Task SaveChangesAsync();
}