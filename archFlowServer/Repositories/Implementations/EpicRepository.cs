using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class EpicRepository : IEpicRepository
{
    private readonly AppDbContext _context;

    public EpicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Epic?> GetByIdAsync(int epicId)
        => await _context.Epics.FirstOrDefaultAsync(e => e.Id == epicId && !e.IsArchived);
    
    public Task<Epic?> GetByIdIncludingArchivedAsync(int id)
        => _context.Epics.FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(Epic epic)
        => await _context.Epics.AddAsync(epic);
    
    public async Task<List<Epic>> GetByBacklogIdAsync(Guid productBacklogId)
    {
        return await _context.Epics
            .Where(e => e.ProductBacklogId == productBacklogId && !e.IsArchived)
            .OrderBy(e => e.Position)
            .ToListAsync();
    }
    
    public async Task<int> GetNextPositionAsync(Guid productBacklogId)
    {
        var max = await _context.Epics
            .Where(e => e.ProductBacklogId == productBacklogId && !e.IsArchived)
            .Select(e => (int?)e.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task ShiftPositionsAsync(Guid productBacklogId, int fromPosition, int toPosition)
    {
        if (fromPosition == toPosition) return;

        // Move para cima (ex.: 7 -> 2): itens [2..6] descem +1
        if (toPosition < fromPosition)
        {
            await _context.Epics
                .Where(e => e.ProductBacklogId == productBacklogId
                            && e.Position >= toPosition
                            && e.Position < fromPosition
                            && !e.IsArchived)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(e => e.Position, e => e.Position + 1));
            return;
        }

        // Move para baixo (ex.: 2 -> 7): itens [3..7] sobem -1
        await _context.Epics
            .Where(e => e.ProductBacklogId == productBacklogId
                        && e.Position > fromPosition
                        && e.Position <= toPosition
                        && !e.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.Position, e => e.Position - 1));
    }
    
    public async Task<int> GetMaxPositionAsync(Guid productBacklogId)
    {
        var max = await _context.Epics
            .Where(e => e.ProductBacklogId == productBacklogId && !e.IsArchived)
            .Select(e => (int?)e.Position)
            .MaxAsync();

        return max ?? -1;
    }

    public async Task SetPositionAsync(int epicId, int position)
    {
        await _context.Epics
            .Where(e => e.Id == epicId && !e.IsArchived)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Position, position));
    }
    
    public async Task ArchiveAsync(int epicId)
    {
        await _context.Epics
            .Where(e => e.Id == epicId && !e.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsArchived, true)
                .SetProperty(e => e.ArchivedAt, DateTime.UtcNow)
                .SetProperty(e => e.UpdatedAt, DateTime.UtcNow));
    }

    public async Task RestoreAsync(int epicId)
    {
        await _context.Epics
            .Where(e => e.Id == epicId && e.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsArchived, false)
                .SetProperty(e => e.ArchivedAt, (DateTime?)null)
                .SetProperty(e => e.UpdatedAt, DateTime.UtcNow));
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
