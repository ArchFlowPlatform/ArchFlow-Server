using agileTrackerServer.Data;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace agileTrackerServer.Repositories.Implementations;

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
            .Include(pb => pb.Epics)
            .ThenInclude(e => e.UserStories)
            .FirstOrDefaultAsync(pb => pb.ProjectId == projectId);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}