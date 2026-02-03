using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IBoardRepository
{
    Task<Board?> GetBySprintIdAsync(Guid projectId, Guid sprintId);
    Task SaveChangesAsync();
}
