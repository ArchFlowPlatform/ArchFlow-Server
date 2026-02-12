using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class CardLabelRepository : ICardLabelRepository
{
    private readonly AppDbContext _context;

    public CardLabelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CardLabel>> GetAllAsync(Guid projectId, int cardId)
    {
        return await _context.CardLabels
            .Include(cl => cl.Label)
            .Where(cl => cl.CardId == cardId && cl.Card.Column.Board.ProjectId == projectId)
            .OrderBy(cl => cl.Label.Name)
            .ToListAsync();
    }

    public Task<CardLabel?> GetByIdAsync(Guid projectId, int cardId, int cardLabelId)
        => _context.CardLabels
            .Include(cl => cl.Label)
            .FirstOrDefaultAsync(cl =>
                cl.Id == cardLabelId &&
                cl.CardId == cardId &&
                cl.Card.Column.Board.ProjectId == projectId);

    public Task<bool> ExistsAsync(int cardId, int labelId)
        => _context.CardLabels.AnyAsync(cl => cl.CardId == cardId && cl.LabelId == labelId);

    public async Task AddAsync(CardLabel cardLabel)
        => await _context.CardLabels.AddAsync(cardLabel);

    public void Remove(CardLabel cardLabel)
        => _context.CardLabels.Remove(cardLabel);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}