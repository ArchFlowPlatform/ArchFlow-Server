using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IUserStoryRepository
{
    Task<UserStory?> GetByIdAsync(int storyId);
    Task AddAsync(UserStory story);
    Task<UserStory?> GetByIdWithEpicAsync(int storyId);
    Task<UserStory?> GetByIdWithEpicIncludingArchivedAsync(int id);
    Task<List<UserStory>> GetByEpicIdAsync(int epicId);
    Task<int> GetNextPositionAsync(int epicId);
    Task ShiftPositionsAsync(int epicId, int fromPosition, int toPosition);
    Task<int> GetMaxPositionAsync(int epicId);
    Task SetPositionAsync(int storyId, int position);
    Task SetEpicAndPositionAsync(int storyId, int epicId, int position);
    Task DecrementPositionsAfterAsync(int epicId, int position);
    Task IncrementPositionsFromAsync(int epicId, int position);
    Task ArchiveAsync(int storyId);
    Task RestoreAsync(int storyId);
    Task ArchiveByEpicIdAsync(int epicId);
    Task RestoreByEpicIdAsync(int epicId);
    Task SaveChangesAsync();
}
