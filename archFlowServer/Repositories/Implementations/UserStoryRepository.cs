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

    public async Task<UserStory?> GetByIdAsync(int storyId)
        => await _context.UserStories.FirstOrDefaultAsync(s => s.Id == storyId && !s.IsArchived);

    public async Task AddAsync(UserStory story)
        => await _context.UserStories.AddAsync(story);
    
    public async Task<UserStory?> GetByIdWithEpicAsync(int storyId)
    {
        return await _context.UserStories
            .Include(s => s.Epic)
            .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsArchived);
    }
    
    public Task<UserStory?> GetByIdWithEpicIncludingArchivedAsync(int id)
        => _context.UserStories
            .Include(s => s.Epic)
            .FirstOrDefaultAsync(s => s.Id == id);
    public async Task<List<UserStory>> GetByEpicIdAsync(int epicId)
    {
        return await _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .OrderBy(s => s.Position)
            .ToListAsync();
    }
    
    public async Task<int> GetNextPositionAsync(int epicId)
    {
        var max = await _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .Select(s => (int?)s.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    // âœ… usado para posiÃ§Ã£o temporÃ¡ria
    public async Task<int> GetMaxPositionAsync(int epicId)
    {
        var max = await _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .Select(s => (int?)s.Position)
            .MaxAsync();

        return max ?? -1;
    }

    // âœ… set direto no banco (sem tracking)
    public async Task SetPositionAsync(int storyId, int position)
    {
        await _context.UserStories
            .Where(s => s.Id == storyId && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.Position, position));
    }

    // âœ… shift por faixa (drag-ready)
    public async Task ShiftPositionsAsync(int epicId, int fromPosition, int toPosition)
    {
        if (fromPosition == toPosition) return;

        // Move para cima: itens [to..from-1] descem +1
        if (toPosition < fromPosition)
        {
            await _context.UserStories
                .Where(s => s.EpicId == epicId
                            && s.Position >= toPosition
                            && s.Position < fromPosition
                            && !s.IsArchived)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(s => s.Position, s => s.Position + 1));
            return;
        }

        // Move para baixo: itens [from+1..to] sobem -1
        await _context.UserStories
            .Where(s => s.EpicId == epicId
                        && s.Position > fromPosition
                        && s.Position <= toPosition
                        && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.Position, s => s.Position - 1));
    }
    
    public async Task SetEpicAndPositionAsync(int storyId, int epicId, int position)
    {
        await _context.UserStories
            .Where(s => s.Id == storyId && !s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.EpicId, epicId)
                .SetProperty(s => s.Position, position));
    }

    // Fecha buraco no Ã©pico de origem: tudo que estÃ¡ depois da posiÃ§Ã£o removida desce -1
    public async Task DecrementPositionsAfterAsync(int epicId, int position)
    {
        await _context.UserStories
            .Where(s => s.EpicId == epicId && s.Position > position && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.Position, s => s.Position - 1));
    }

    // Abre espaÃ§o no Ã©pico de destino: tudo >= toPosition sobe +1
    public async Task IncrementPositionsFromAsync(int epicId, int position)
    {
        await _context.UserStories
            .Where(s => s.EpicId == epicId && s.Position >= position && !s.IsArchived)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.Position, s => s.Position + 1));
    }
    
    public async Task ArchiveAsync(int storyId)
    {
        await _context.UserStories
            .Where(s => s.Id == storyId && !s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, true)
                .SetProperty(s => s.ArchivedAt, DateTime.UtcNow)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));
    }

    public async Task RestoreAsync(int storyId)
    {
        await _context.UserStories
            .Where(s => s.Id == storyId && s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, false)
                .SetProperty(s => s.ArchivedAt, (DateTime?)null)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));
    }

    public async Task ArchiveByEpicIdAsync(int epicId)
    {
        await _context.UserStories
            .Where(s => s.EpicId == epicId && !s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, true)
                .SetProperty(s => s.ArchivedAt, DateTime.UtcNow)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));
    }
    
    public async Task RestoreByEpicIdAsync(int epicId)
    {
        await _context.UserStories
            .Where(s => s.EpicId == epicId && s.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.IsArchived, false)
                .SetProperty(s => s.ArchivedAt, (DateTime?)null)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow));
    }
    
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
