using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IUserStoryRepository
{
    Task<UserStory?> GetByIdAsync(int storyId);
    Task<UserStory?> GetByIdWithEpicAsync(int storyId);
    Task<UserStory?> GetByIdWithEpicIncludingArchivedAsync(int id);
    Task<UserStory?> GetByIdWithEpicAndBacklogAsync(int storyId);

    Task<List<UserStory>> GetByEpicIdAsync(int epicId);

    Task AddAsync(UserStory story);

    // Backlog ordering (inside Epic)
    Task<int> GetNextBacklogPositionAsync(int epicId);
    Task<int> GetMaxBacklogPositionAsync(int epicId);

    Task SetBacklogPositionAsync(int storyId, int position);
    Task ShiftBacklogPositionsAsync(int epicId, int fromPosition, int toPosition);

    Task SetEpicAndBacklogPositionAsync(int storyId, int epicId, int position);
    Task DecrementBacklogPositionsAfterAsync(int epicId, int position);
    Task IncrementBacklogPositionsFromAsync(int epicId, int position);

    // Archive / Restore
    Task ArchiveAsync(int storyId);
    Task RestoreAsync(int storyId);
    Task ArchiveByEpicIdAsync(int epicId);
    Task RestoreByEpicIdAsync(int epicId);

    Task SaveChangesAsync();
}
