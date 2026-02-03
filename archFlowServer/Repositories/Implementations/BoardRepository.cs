using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;

    public BoardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Board?> GetBySprintIdAsync(Guid projectId, Guid sprintId)
    {
        return await _context.Boards
            .Include(b => b.Sprint)
            .FirstOrDefaultAsync(b =>
                b.ProjectId == projectId &&
                b.SprintId == sprintId &&
                !b.Sprint.IsArchived
            );
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
