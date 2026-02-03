using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class BoardCardRepository : IBoardCardRepository
{
    private readonly AppDbContext _context;

    public BoardCardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BoardCard>> GetAllByColumnAsync(Guid projectId, Guid sprintId, int columnId)
    {
        return await _context.BoardCards
            .AsNoTracking()
            .Where(c =>
                c.ColumnId == columnId &&
                c.Column.Board.SprintId == sprintId &&
                c.Column.Board.ProjectId == projectId
            )
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<BoardCard?> GetByIdAsync(Guid projectId, Guid sprintId, int cardId)
    {
        return await _context.BoardCards
            .FirstOrDefaultAsync(c =>
                c.Id == cardId &&
                c.Column.Board.SprintId == sprintId &&
                c.Column.Board.ProjectId == projectId
            );
    }

    public async Task<int> GetNextPositionAsync(int columnId)
    {
        var max = await _context.BoardCards
            .Where(c => c.ColumnId == columnId)
            .Select(c => (int?)c.Position)
            .MaxAsync();

        return (max ?? -1) + 1;
    }

    public async Task<int> GetMaxPositionAsync(int columnId)
    {
        var max = await _context.BoardCards
            .Where(c => c.ColumnId == columnId)
            .Select(c => (int?)c.Position)
            .MaxAsync();

        return max ?? -1;
    }

    public async Task IncrementPositionsFromAsync(int columnId, int position)
    {
        await _context.BoardCards
            .Where(c => c.ColumnId == columnId && c.Position >= position)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.Position, c => c.Position + 1));
    }

    public async Task DecrementPositionsAfterAsync(int columnId, int position)
    {
        await _context.BoardCards
            .Where(c => c.ColumnId == columnId && c.Position > position)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.Position, c => c.Position - 1));
    }

    public async Task SetColumnAsync(int cardId, int toColumnId)
    {
        await _context.BoardCards
            .Where(c => c.Id == cardId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.ColumnId, toColumnId));
    }

    public async Task SetPositionAsync(int cardId, int position)
    {
        await _context.BoardCards
            .Where(c => c.Id == cardId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.Position, position));
    }

    public async Task AddAsync(BoardCard card)
        => await _context.BoardCards.AddAsync(card);

    public void Remove(BoardCard card)
        => _context.BoardCards.Remove(card);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
