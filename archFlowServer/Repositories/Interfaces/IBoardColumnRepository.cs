using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IBoardColumnRepository
{
    Task<IReadOnlyList<BoardColumn>> GetAllAsync(Guid projectId, Guid sprintId);
    Task<BoardColumn?> GetByIdAsync(Guid projectId, Guid sprintId, int columnId);

    Task<int> GetNextPositionAsync(Guid boardId);
    Task<int> GetMaxPositionAsync(Guid boardId);

    Task IncrementPositionsFromAsync(Guid boardId, int position);
    Task DecrementPositionsAfterAsync(Guid boardId, int position);

    Task AddAsync(BoardColumn column);
    void Remove(BoardColumn column);

    Task SaveChangesAsync();
}
