using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface ICardLabelRepository
{
    Task<IReadOnlyList<CardLabel>> GetAllAsync(Guid projectId, int cardId);
    Task<CardLabel?> GetByIdAsync(Guid projectId, int cardId, int cardLabelId);

    Task<bool> ExistsAsync(int cardId, int labelId);

    Task AddAsync(CardLabel cardLabel);
    void Remove(CardLabel cardLabel);

    Task SaveChangesAsync();
}