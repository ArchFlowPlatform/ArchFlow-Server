using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface ICardAttachmentRepository
{
    Task<IReadOnlyList<CardAttachment>> GetAllAsync(Guid projectId, int cardId);
    Task<CardAttachment?> GetByIdAsync(Guid projectId, int cardId, int attachmentId);

    Task AddAsync(CardAttachment attachment);
    void Remove(CardAttachment attachment);

    Task SaveChangesAsync();
}