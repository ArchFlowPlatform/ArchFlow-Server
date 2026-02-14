using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface ICardCommentRepository
{
    Task<IReadOnlyList<CardComment>> GetAllAsync(Guid projectId, int cardId);
    Task<CardComment?> GetByIdAsync(Guid projectId, int cardId, int commentId);

    Task<bool> ParentExistsOnSameCardAsync(int cardId, int parentCommentId);

    Task AddAsync(CardComment comment);
    void Remove(CardComment comment);

    Task SaveChangesAsync();
}