using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface ISprintRepository
{
    Task<IReadOnlyList<Sprint>> GetAllActiveAsync(Guid projectId);
    Task<IReadOnlyList<Sprint>> GetAllAsync(Guid projectId, bool includeArchived);

    Task<Sprint?> GetActiveByIdAsync(Guid projectId, Guid sprintId);
    Task<Sprint?> GetArchivedByIdAsync(Guid projectId, Guid sprintId);

    // utilitário para regra: 1 sprint ativa por projeto
    Task<bool> HasAnotherActiveSprintAsync(Guid projectId, Guid excludeSprintId);

    Task AddAsync(Sprint sprint);
    Task AddBoardAsync(Board board);

    Task SaveChangesAsync();
}