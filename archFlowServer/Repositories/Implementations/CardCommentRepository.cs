using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class CardCommentRepository : ICardCommentRepository
{
    private readonly AppDbContext _context;

    public CardCommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CardComment>> GetAllAsync(Guid projectId, int cardId)
    {
        return await _context.CardComments
            .Where(c => c.CardId == cardId && c.Card.Column.Board.ProjectId == projectId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public Task<CardComment?> GetByIdAsync(Guid projectId, int cardId, int commentId)
        => _context.CardComments
            .FirstOrDefaultAsync(c =>
                c.Id == commentId &&
                c.CardId == cardId &&
                c.Card.Column.Board.ProjectId == projectId);

    public Task<bool> ParentExistsOnSameCardAsync(int cardId, int parentCommentId)
        => _context.CardComments.AnyAsync(c =>
            c.Id == parentCommentId &&
            c.CardId == cardId);

    public async Task AddAsync(CardComment comment)
        => await _context.CardComments.AddAsync(comment);

    public void Remove(CardComment comment)
        => _context.CardComments.Remove(comment);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}