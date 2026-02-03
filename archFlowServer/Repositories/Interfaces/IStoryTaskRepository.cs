using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IStoryTaskRepository
{
    Task<IReadOnlyList<StoryTask>> GetAllByUserStoryAsync(Guid projectId, int userStoryId);
    Task<StoryTask?> GetByIdAsync(Guid projectId, int userStoryId, int taskId);
    Task<StoryTask?> GetByIdInProjectAsync(Guid projectId, int taskId);
    Task AddAsync(StoryTask task);
    void Remove(StoryTask task);

    Task SaveChangesAsync();
}
