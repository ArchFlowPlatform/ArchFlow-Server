using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface IEpicRepository
{
    Task<Epic?> GetByIdAsync(int epicId);
    Task<Epic?> GetByIdIncludingArchivedAsync(int epicId);
    Task AddAsync(Epic epic);
    Task<List<Epic>> GetByBacklogIdAsync(Guid productBacklogId);
    Task<int> GetNextPositionAsync(Guid productBacklogId);
    Task ShiftPositionsAsync(Guid productBacklogId, int fromPosition, int toPosition);
    Task<int> GetMaxPositionAsync(Guid productBacklogId);
    Task SetPositionAsync(int epicId, int position);
    Task ArchiveAsync(int epicId);
    Task RestoreAsync(int epicId);
    Task SaveChangesAsync();
}
