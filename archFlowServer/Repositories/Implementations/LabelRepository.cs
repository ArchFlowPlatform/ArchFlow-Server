using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class LabelRepository : ILabelRepository
{
    private readonly AppDbContext _context;

    public LabelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Label>> GetAllAsync(Guid projectId)
    {
        return await _context.Labels
            .Where(l => l.ProjectId == projectId)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public Task<Label?> GetByIdAsync(Guid projectId, int labelId)
        => _context.Labels.FirstOrDefaultAsync(l => l.ProjectId == projectId && l.Id == labelId);

    public async Task<bool> ExistsByNameAsync(Guid projectId, string normalizedName, int? excludeLabelId = null)
    {
        var query = _context.Labels.Where(l =>
            l.ProjectId == projectId &&
            l.Name.ToLower() == normalizedName);

        if (excludeLabelId.HasValue)
            query = query.Where(l => l.Id != excludeLabelId.Value);

        return await query.AnyAsync();
    }

    public async Task AddAsync(Label label)
        => await _context.Labels.AddAsync(label);

    public void Remove(Label label)
        => _context.Labels.Remove(label);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}