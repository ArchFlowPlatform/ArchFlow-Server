using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IStoryTaskRepository
{
    Task<IReadOnlyList<StoryTask>> GetAllByUserStoryAsync(Guid projectId, int userStoryId);
    Task<StoryTask?> GetByIdAsync(Guid projectId, int userStoryId, int taskId);
    Task<StoryTask?> GetByIdInProjectAsync(Guid projectId, int taskId);

    Task<int> GetNextPositionAsync(int userStoryId);
    Task<int> GetMaxPositionAsync(int userStoryId);

    Task SetPositionAsync(int taskId, int position);
    Task ShiftPositionsAsync(int userStoryId, int fromPosition, int toPosition);

    Task SetUserStoryAndPositionAsync(int taskId, int userStoryId, int position);
    Task DecrementPositionsAfterAsync(int userStoryId, int position);
    Task IncrementPositionsFromAsync(int userStoryId, int position);

    Task AddAsync(StoryTask task);
    void Remove(StoryTask task);

    Task SaveChangesAsync();
}