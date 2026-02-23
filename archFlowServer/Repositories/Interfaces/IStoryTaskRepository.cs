using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IStoryTaskRepository
{
    Task<IReadOnlyList<StoryTask>> GetAllBySprintItemAsync(Guid projectId, int sprintItemId);
    Task<StoryTask?> GetByIdAsync(Guid projectId, int sprintItemId, int taskId);

    Task<int> GetNextPositionAsync(int sprintItemId);
    Task<int> GetMaxPositionAsync(int sprintItemId);

    Task SetPositionAsync(int taskId, int position);
    Task ShiftPositionsAsync(int sprintItemId, int fromPosition, int toPosition);

    Task SetSprintItemAndPositionAsync(int taskId, int sprintItemId, int position);
    Task DecrementPositionsAfterAsync(int sprintItemId, int position);
    Task IncrementPositionsFromAsync(int sprintItemId, int position);

    Task AddAsync(StoryTask task);
    void Remove(StoryTask task);

    Task SaveChangesAsync();
}