using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class UserStoryRepository : IUserStoryRepository
{
    private readonly AppDbContext _context;

    public UserStoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<UserStory?> GetByIdAsync(int storyId)
        => _context.UserStories.FirstOrDefaultAsync(s => s.Id == storyId && !s.IsArchived);

    public Task AddAsync(UserStory story)
        => _context.UserStories.AddAsync(story).AsTask();

    public Task<UserStory?> GetByIdWithEpicAsync(int storyId)
        => _context.UserStories
            .Include(s => s.Epic)
            .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsArchived);

    public Task<UserStory?> GetByIdWithEpicIncludingArchivedAsync(int id)
        => _context.UserStories
            .Include(s => s.Epic)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<UserStory>> GetByEpicIdAsync(int epicId)
        => _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .OrderBy(s => s.BacklogPosition)
            .ToListAsync();

    public Task<UserStory?> GetByIdWithEpicAndBacklogAsync(int storyId)
        => _context.UserStories
            .Include(s => s.Epic)
            .ThenInclude(e => e.ProductBacklog)
            .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsArchived);

    public async Task<int> GetNextBacklogPositionAsync(int epicId)
    {
        var max = await _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .Select(s => (int?)s.BacklogPosition)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task<int> GetMaxBacklogPositionAsync(int epicId)
    {
        var max = await _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .Select(s => (int?)s.BacklogPosition)
            .MaxAsync();

        return max ?? -1;
    }

    public Task SetBacklogPositionAsync(int storyId, int position)
        => _context.UserStories
            .Where(s => s.Id == storyId && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.BacklogPosition, position));

    public async Task ShiftBacklogPositionsAsync(int epicId, int fromPosition, int toPosition)
    {
        if (fromPosition == toPosition) return;

        if (toPosition < fromPosition)
        {
            await _context.UserStories
                .Where(s => s.EpicId == epicId
                            && s.BacklogPosition >= toPosition
                            && s.BacklogPosition < fromPosition
                            && !s.IsArchived)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(s => s.BacklogPosition, s => s.BacklogPosition + 1));
            return;
        }

        await _context.UserStories
            .Where(s => s.EpicId == epicId
                        && s.BacklogPosition > fromPosition
                        && s.BacklogPosition <= toPosition
                        && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.BacklogPosition, s => s.BacklogPosition - 1));
    }

    public Task SetEpicAndBacklogPositionAsync(int storyId, int epicId, int position)
        => _context.UserStories
            .Where(s => s.Id == storyId && !s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.EpicId, epicId)
                .SetProperty(s => s.BacklogPosition, position)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));

    public Task DecrementBacklogPositionsAfterAsync(int epicId, int position)
        => _context.UserStories
            .Where(s => s.EpicId == epicId && s.BacklogPosition > position && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.BacklogPosition, s => s.BacklogPosition - 1));

    public Task IncrementBacklogPositionsFromAsync(int epicId, int position)
        => _context.UserStories
            .Where(s => s.EpicId == epicId && s.BacklogPosition >= position && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.BacklogPosition, s => s.BacklogPosition + 1));

    public Task ArchiveAsync(int storyId)
        => _context.UserStories
            .Where(s => s.Id == storyId && !s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, true)
                .SetProperty(s => s.ArchivedAt, DateTime.UtcNow)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));

    public Task RestoreAsync(int storyId)
        => _context.UserStories
            .Where(s => s.Id == storyId && s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, false)
                .SetProperty(s => s.ArchivedAt, (DateTime?)null)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));

    public Task ArchiveByEpicIdAsync(int epicId)
        => _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, true)
                .SetProperty(s => s.ArchivedAt, DateTime.UtcNow)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));

    public Task RestoreByEpicIdAsync(int epicId)
        => _context.UserStories
            .Where(s => s.EpicId == epicId && s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, false)
                .SetProperty(s => s.ArchivedAt, (DateTime?)null)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();
}
