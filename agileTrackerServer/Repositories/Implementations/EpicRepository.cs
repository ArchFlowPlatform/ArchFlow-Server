using agileTrackerServer.Data;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace agileTrackerServer.Repositories.Implementations;

public class EpicRepository : IEpicRepository
{
    private readonly AppDbContext _context;

    public EpicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Epic?> GetByIdAsync(Guid epicId)
        => await _context.Epics.FirstOrDefaultAsync(e => e.Id == epicId);

    public async Task AddAsync(Epic epic)
        => await _context.Epics.AddAsync(epic);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}