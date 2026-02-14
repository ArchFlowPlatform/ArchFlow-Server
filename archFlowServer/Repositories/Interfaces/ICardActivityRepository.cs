using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;

namespace archFlowServer.Repositories.Interfaces;

public interface ICardActivityRepository
{
    Task<IReadOnlyList<CardActivity>> GetAllAsync(Guid projectId, int cardId, int take = 50);
    Task AddAsync(CardActivity activity);

    Task SaveChangesAsync();
}