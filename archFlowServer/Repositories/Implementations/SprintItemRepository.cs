using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class SprintItemRepository : ISprintItemRepository
{
    private readonly AppDbContext _context;

    public SprintItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SprintItem>> GetAllBySprintAsync(Guid projectId, Guid sprintId)
    {
        return await _context.SprintItems
            .AsNoTracking()
            .Include(si => si.UserStory)
            .ThenInclude(us => us.Epic)
            .Where(si =>
                si.SprintId == sprintId &&
                si.Sprint.ProjectId == projectId
            )
            .OrderBy(si => si.Position)
            .ThenBy(si => si.Id)
            .ToListAsync();
    }

    public async Task<SprintItem?> GetByIdAsync(Guid projectId, Guid sprintId, int sprintItemId)
    {
        return await _context.SprintItems
            .AsNoTracking()
            .Include(si => si.UserStory)
            .ThenInclude(us => us.Epic)
            .FirstOrDefaultAsync(si =>
                si.Id == sprintItemId &&
                si.SprintId == sprintId &&
                si.Sprint.ProjectId == projectId
            );
    }

    public async Task<bool> StoryBelongsToProjectAsync(Guid projectId, int userStoryId)
    {
        // join UserStory -> Epic -> ProductBacklog -> ProjectId
        return await _context.UserStories
            .Where(us => us.Id == userStoryId)
            .Join(_context.Epics,
                us => us.EpicId,
                e => e.Id,
                (us, e) => new { us, e })
            .Join(_context.ProductBacklogs,
                x => x.e.ProductBacklogId,
                pb => pb.Id,
                (x, pb) => new { x.us, x.e, pb })
            .AnyAsync(x => x.pb.ProjectId == projectId);
    }

    public async Task<int> GetNextPositionAsync(Guid sprintId)
    {
        var max = await _context.SprintItems
            .Where(si => si.SprintId == sprintId)
            .Select(si => (int?)si.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task<int> GetMaxPositionAsync(Guid sprintId)
    {
        var max = await _context.SprintItems
            .Where(si => si.SprintId == sprintId)
            .Select(si => (int?)si.Position)
            .MaxAsync();

        return max ?? -1;
    }

    public async Task IncrementPositionsFromAsync(Guid sprintId, int position)
    {
        await _context.SprintItems
            .Where(si => si.SprintId == sprintId && si.Position >= position)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(si => si.Position, si => si.Position + 1));
    }

    public async Task DecrementPositionsAfterAsync(Guid sprintId, int position)
    {
        await _context.SprintItems
            .Where(si => si.SprintId == sprintId && si.Position > position)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(si => si.Position, si => si.Position - 1));
    }

    public async Task ShiftPositionsAsync(Guid sprintId, int fromPosition, int toPosition)
    {
        if (fromPosition == toPosition) return;

        // Move para cima: itens [to..from-1] descem +1
        if (toPosition < fromPosition)
        {
            await _context.SprintItems
                .Where(si =>
                    si.SprintId == sprintId &&
                    si.Position >= toPosition &&
                    si.Position < fromPosition
                )
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(si => si.Position, si => si.Position + 1));

            return;
        }

        // Move para baixo: itens [from+1..to] sobem -1
        await _context.SprintItems
            .Where(si =>
                si.SprintId == sprintId &&
                si.Position > fromPosition &&
                si.Position <= toPosition
            )
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(si => si.Position, si => si.Position - 1));
    }

    public async Task SetPositionAsync(int sprintItemId, int position)
    {
        await _context.SprintItems
            .Where(si => si.Id == sprintItemId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(si => si.Position, position));
    }

    public async Task SetNotesAsync(int sprintItemId, string notes)
    {
        await _context.SprintItems
            .Where(si => si.Id == sprintItemId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(si => si.Notes, notes));
    }

    public async Task AddAsync(SprintItem sprintItem)
        => await _context.SprintItems.AddAsync(sprintItem);

    public void Remove(SprintItem sprintItem)
        => _context.SprintItems.Remove(sprintItem);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
