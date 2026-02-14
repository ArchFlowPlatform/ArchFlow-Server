using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class CardActivityRepository : ICardActivityRepository
{
    private readonly AppDbContext _context;

    public CardActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CardActivity>> GetAllAsync(Guid projectId, int cardId, int take = 50)
    {
        return await _context.CardActivities
            .Where(a => a.CardId == cardId && a.Card.Column.Board.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task AddAsync(CardActivity activity)
        => await _context.CardActivities.AddAsync(activity);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}