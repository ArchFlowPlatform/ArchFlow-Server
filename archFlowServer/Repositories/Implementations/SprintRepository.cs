using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class SprintRepository : ISprintRepository
{
    private readonly AppDbContext _context;

    public SprintRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Sprint>> GetAllActiveAsync(Guid projectId)
    {
        return await _context.Sprints
            .Include(s => s.Board) // 1:1 (se existir navigation)
            .Where(s =>
                s.ProjectId == projectId &&
                !s.IsArchived
            )
            .OrderByDescending(s => s.StartDate)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Sprint>> GetAllAsync(Guid projectId, bool includeArchived)
    {
        var query = _context.Sprints
            .Include(s => s.Board) // 1:1 (se existir navigation)
            .Where(s => s.ProjectId == projectId);

        if (!includeArchived)
            query = query.Where(s => !s.IsArchived);

        return await query
            .OrderByDescending(s => s.StartDate)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Sprint?> GetActiveByIdAsync(Guid projectId, Guid sprintId)
    {
        return await _context.Sprints
            .Include(s => s.Board) // 1:1 (se existir navigation)
            .FirstOrDefaultAsync(s =>
                s.ProjectId == projectId &&
                s.Id == sprintId &&
                !s.IsArchived
            );
    }

    public async Task<Sprint?> GetArchivedByIdAsync(Guid projectId, Guid sprintId)
    {
        return await _context.Sprints
            .Include(s => s.Board) // 1:1 (se existir navigation)
            .FirstOrDefaultAsync(s =>
                s.ProjectId == projectId &&
                s.Id == sprintId &&
                s.IsArchived
            );
    }

    public async Task<bool> HasAnotherActiveSprintAsync(Guid projectId, Guid excludeSprintId)
    {
        return await _context.Sprints.AnyAsync(s =>
            s.ProjectId == projectId &&
            !s.IsArchived &&
            s.Status == SprintStatus.Active &&
            s.Id != excludeSprintId
        );
    }

    public async Task AddAsync(Sprint sprint)
    {
        await _context.Sprints.AddAsync(sprint);
    }

    public async Task AddBoardAsync(Board board)
    {
        await _context.Boards.AddAsync(board);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
