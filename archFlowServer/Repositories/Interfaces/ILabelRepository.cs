using archFlowServer.Models.Entities;

namespace archFlowServer.Repositories.Interfaces;

public interface ILabelRepository
{
    Task<IReadOnlyList<Label>> GetAllAsync(Guid projectId);
    Task<Label?> GetByIdAsync(Guid projectId, int labelId);

    Task<bool> ExistsByNameAsync(Guid projectId, string normalizedName, int? excludeLabelId = null);

    Task AddAsync(Label label);
    void Remove(Label label);

    Task SaveChangesAsync();
}