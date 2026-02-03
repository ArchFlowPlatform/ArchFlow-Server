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
            .OrderBy(t => t.Priority)
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


    public async Task AddAsync(StoryTask task)
        => await _context.StoryTasks.AddAsync(task);

    public void Remove(StoryTask task)
        => _context.StoryTasks.Remove(task);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
