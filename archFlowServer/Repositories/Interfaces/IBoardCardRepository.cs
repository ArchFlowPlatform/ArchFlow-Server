using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IBoardCardRepository
{
    Task<IReadOnlyList<BoardCard>> GetAllByColumnAsync(Guid projectId, Guid sprintId, int columnId);
    Task<BoardCard?> GetByIdAsync(Guid projectId, Guid sprintId, int cardId);

    Task<int> GetNextPositionAsync(int columnId);
    Task<int> GetMaxPositionAsync(int columnId);

    Task SetPositionAsync(int cardId, int position);
    Task SetColumnAsync(int cardId, int toColumnId);

    Task ShiftPositionsAsync(int columnId, int fromPosition, int toPosition);
    Task IncrementPositionsFromAsync(int columnId, int position);
    Task DecrementPositionsAfterAsync(int columnId, int position);

    Task AddAsync(BoardCard card);
    void Remove(BoardCard card);
    Task SaveChangesAsync();
}