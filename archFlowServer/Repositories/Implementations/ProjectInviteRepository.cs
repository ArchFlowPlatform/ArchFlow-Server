using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class ProjectInviteRepository : IProjectInviteRepository
{
    private readonly AppDbContext _context;

    public ProjectInviteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ProjectInvite invite)
    {
        await _context.Set<ProjectInvite>().AddAsync(invite);
    }

    public async Task<ProjectInvite?> GetByTokenAsync(string token)
    {
        return await _context.Set<ProjectInvite>()
            .FirstOrDefaultAsync(i =>
                i.Token == token &&
                !i.Accepted
            );
    }

    public async Task<bool> ExistsActiveInviteAsync(
        Guid projectId,
        string email)
    {
        var now = DateTime.UtcNow;

        return await _context.Set<ProjectInvite>()
            .AnyAsync(i =>
                i.ProjectId == projectId &&
                i.Email == email.ToLower() &&
                !i.Accepted &&
                i.ExpiresAt > now
            );
    }

    public void Delete(ProjectInvite invite)
    {
        _context.Set<ProjectInvite>().Remove(invite);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
