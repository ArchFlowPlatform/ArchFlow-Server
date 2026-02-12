using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class CardAttachmentRepository : ICardAttachmentRepository
{
    private readonly AppDbContext _context;

    public CardAttachmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CardAttachment>> GetAllAsync(Guid projectId, int cardId)
    {
        return await _context.CardAttachments
            .Where(a => a.CardId == cardId && a.Card.Column.Board.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public Task<CardAttachment?> GetByIdAsync(Guid projectId, int cardId, int attachmentId)
        => _context.CardAttachments.FirstOrDefaultAsync(a =>
            a.Id == attachmentId &&
            a.CardId == cardId &&
            a.Card.Column.Board.ProjectId == projectId);

    public async Task AddAsync(CardAttachment attachment)
        => await _context.CardAttachments.AddAsync(attachment);

    public void Remove(CardAttachment attachment)
        => _context.CardAttachments.Remove(attachment);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}