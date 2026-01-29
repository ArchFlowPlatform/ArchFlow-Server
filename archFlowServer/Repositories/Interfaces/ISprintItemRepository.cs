using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface ISprintItemRepository
{
    Task<IReadOnlyList<SprintItem>> GetAllBySprintAsync(Guid projectId, Guid sprintId);
    Task<SprintItem?> GetByIdAsync(Guid projectId, Guid sprintId, int sprintItemId);

    Task<bool> StoryBelongsToProjectAsync(Guid projectId, int userStoryId);

    Task<int> GetNextPositionAsync(Guid sprintId);
    Task<int> GetMaxPositionAsync(Guid sprintId);

    Task IncrementPositionsFromAsync(Guid sprintId, int position);
    Task DecrementPositionsAfterAsync(Guid sprintId, int position);
    Task ShiftPositionsAsync(Guid sprintId, int fromPosition, int toPosition);

    Task SetPositionAsync(int sprintItemId, int position);
    Task SetNotesAsync(int sprintItemId, string notes);

    Task AddAsync(SprintItem sprintItem);
    void Remove(SprintItem sprintItem);

    Task SaveChangesAsync();
}
