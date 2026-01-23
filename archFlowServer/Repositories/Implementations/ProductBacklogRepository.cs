using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class ProductBacklogRepository : IProductBacklogRepository
{
    private readonly AppDbContext _context;

    public ProductBacklogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductBacklog?> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProductBacklogs
            .Include(pb => pb.Epics.Where(e => !e.IsArchived))
            .ThenInclude(e => e.UserStories.Where(s => !s.IsArchived))
            .FirstOrDefaultAsync(pb => pb.ProjectId == projectId);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
