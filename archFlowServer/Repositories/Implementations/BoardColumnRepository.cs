using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class BoardColumnRepository : IBoardColumnRepository
{
    private readonly AppDbContext _context;

    public BoardColumnRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BoardColumn>> GetAllAsync(Guid projectId, Guid sprintId)
    {
        return await _context.BoardColumns
            .AsNoTracking()
            .Where(c =>
                c.Board.SprintId == sprintId &&
                c.Board.ProjectId == projectId
            )
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<BoardColumn?> GetByIdAsync(Guid projectId, Guid sprintId, int columnId)
    {
        return await _context.BoardColumns
            .FirstOrDefaultAsync(c =>
                c.Id == columnId &&
                c.Board.SprintId == sprintId &&
                c.Board.ProjectId == projectId
            );
    }

    public async Task<int> GetNextPositionAsync(Guid boardId)
    {
        var max = await _context.BoardColumns
            .Where(c => c.BoardId == boardId)
            .Select(c => (int?)c.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task<int> GetMaxPositionAsync(Guid boardId)
    {
        var max = await _context.BoardColumns
            .Where(c => c.BoardId == boardId)
            .Select(c => (int?)c.Position)
            .MaxAsync();

        return max ?? -1;
    }

    public async Task IncrementPositionsFromAsync(Guid boardId, int position)
    {
        await _context.BoardColumns
            .Where(c => c.BoardId == boardId && c.Position >= position)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.Position, c => c.Position + 1));
    }

    public async Task DecrementPositionsAfterAsync(Guid boardId, int position)
    {
        await _context.BoardColumns
            .Where(c => c.BoardId == boardId && c.Position > position)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.Position, c => c.Position - 1));
    }

    public async Task AddAsync(BoardColumn column)
        => await _context.BoardColumns.AddAsync(column);

    public void Remove(BoardColumn column)
        => _context.BoardColumns.Remove(column);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
