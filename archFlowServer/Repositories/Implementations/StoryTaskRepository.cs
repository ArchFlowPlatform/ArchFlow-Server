using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class StoryTaskRepository : IStoryTaskRepository
{
    private readonly AppDbContext _context;

    public StoryTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoryTask>> GetAllByUserStoryAsync(Guid projectId, int userStoryId)
    {
        return await _context.StoryTasks
            .AsNoTracking()
            .Where(t =>
                t.UserStoryId == userStoryId &&
                t.UserStory.Epic.ProductBacklog.ProjectId == projectId
            )
            .OrderBy(t => t.Position)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<StoryTask?> GetByIdAsync(Guid projectId, int userStoryId, int taskId)
    {
        return await _context.StoryTasks
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.UserStoryId == userStoryId &&
                t.UserStory.Epic.ProductBacklog.ProjectId == projectId
            );
    }

    public async Task<StoryTask?> GetByIdInProjectAsync(Guid projectId, int taskId)
    {
        return await _context.StoryTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.UserStory.Epic.ProductBacklog.ProjectId == projectId
            );
    }

    public async Task<int> GetNextPositionAsync(int userStoryId)
    {
        var max = await _context.StoryTasks
            .Where(t => t.UserStoryId == userStoryId)
            .Select(t => (int?)t.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task<int> GetMaxPositionAsync(int userStoryId)
    {
        var max = await _context.StoryTasks
            .Where(t => t.UserStoryId == userStoryId)
            .Select(t => (int?)t.Position)
            .MaxAsync();

        return max ?? -1;
    }

    public async Task SetPositionAsync(int taskId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.Id == taskId)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(t => t.Position, position)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
    }

    public async Task ShiftPositionsAsync(int userStoryId, int fromPosition, int toPosition)
    {
        if (fromPosition == toPosition) return;

        if (toPosition < fromPosition)
        {
            // Move para cima: itens [to..from-1] descem +1
            await _context.StoryTasks
                .Where(t => t.UserStoryId == userStoryId
                            && t.Position >= toPosition
                            && t.Position < fromPosition)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(t => t.Position, t => t.Position + 1)
                        .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
            return;
        }

        // Move para baixo: itens [from+1..to] sobem -1
        await _context.StoryTasks
            .Where(t => t.UserStoryId == userStoryId
                        && t.Position > fromPosition
                        && t.Position <= toPosition)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(t => t.Position, t => t.Position - 1)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
    }

    public async Task SetUserStoryAndPositionAsync(int taskId, int userStoryId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.Id == taskId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.UserStoryId, userStoryId)
                .SetProperty(t => t.Position, position)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
    }

    public async Task DecrementPositionsAfterAsync(int userStoryId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.UserStoryId == userStoryId && t.Position > position)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(t => t.Position, t => t.Position - 1)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
    }

    public async Task IncrementPositionsFromAsync(int userStoryId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.UserStoryId == userStoryId && t.Position >= position)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(t => t.Position, t => t.Position + 1)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
    }

    public async Task AddAsync(StoryTask task)
        => await _context.StoryTasks.AddAsync(task);

    public void Remove(StoryTask task)
        => _context.StoryTasks.Remove(task);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
