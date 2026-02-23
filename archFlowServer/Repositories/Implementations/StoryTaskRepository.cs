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

    public async Task<IReadOnlyList<StoryTask>> GetAllBySprintItemAsync(Guid projectId, int sprintItemId)
    {
        return await _context.StoryTasks
            .AsNoTracking()
            .Where(t =>
                t.SprintItemId == sprintItemId &&
                t.SprintItem.Sprint.ProjectId == projectId
            )
            .OrderBy(t => t.Position)
            .ToListAsync();
    }

    public async Task<StoryTask?> GetByIdAsync(Guid projectId, int sprintItemId, int taskId)
    {
        return await _context.StoryTasks
            .Where(t =>
                t.Id == taskId &&
                t.SprintItemId == sprintItemId &&
                t.SprintItem.Sprint.ProjectId == projectId
            )
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetNextPositionAsync(int sprintItemId)
    {
        // next = max + 1 (se vazio => 0)
        var max = await _context.StoryTasks
            .Where(t => t.SprintItemId == sprintItemId)
            .Select(t => (int?)t.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task<int> GetMaxPositionAsync(int sprintItemId)
    {
        var max = await _context.StoryTasks
            .Where(t => t.SprintItemId == sprintItemId)
            .Select(t => (int?)t.Position)
            .MaxAsync();

        return max ?? -1;
    }

    public async Task SetPositionAsync(int taskId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.Id == taskId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Position, position)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow)
            );
    }

    /// <summary>
    /// Shift de positions no mesmo SprintItem, movendo os demais itens para abrir/fechar espaço.
    /// - se from < to: itens em (from+1..to) decrementam
    /// - se from > to: itens em (to..from-1) incrementam
    /// </summary>
    public async Task ShiftPositionsAsync(int sprintItemId, int fromPosition, int toPosition)
    {
        if (fromPosition < toPosition)
        {
            await _context.StoryTasks
                .Where(t => t.SprintItemId == sprintItemId
                            && t.Position > fromPosition
                            && t.Position <= toPosition)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Position, t => t.Position - 1)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow)
                );
        }
        else
        {
            await _context.StoryTasks
                .Where(t => t.SprintItemId == sprintItemId
                            && t.Position >= toPosition
                            && t.Position < fromPosition)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Position, t => t.Position + 1)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow)
                );
        }
    }

    public async Task SetSprintItemAndPositionAsync(int taskId, int sprintItemId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.Id == taskId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.SprintItemId, sprintItemId)
                .SetProperty(t => t.Position, position)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow)
            );
    }

    public async Task DecrementPositionsAfterAsync(int sprintItemId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.SprintItemId == sprintItemId && t.Position > position)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Position, t => t.Position - 1)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow)
            );
    }

    public async Task IncrementPositionsFromAsync(int sprintItemId, int position)
    {
        await _context.StoryTasks
            .Where(t => t.SprintItemId == sprintItemId && t.Position >= position)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Position, t => t.Position + 1)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow)
            );
    }

    public async Task AddAsync(StoryTask task)
    {
        await _context.StoryTasks.AddAsync(task);
    }

    public void Remove(StoryTask task)
    {
        _context.StoryTasks.Remove(task);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}