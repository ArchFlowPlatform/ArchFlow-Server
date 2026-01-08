using agileTrackerServer.Data;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Models.Enums;
using agileTrackerServer.Models.Exceptions;
using agileTrackerServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace agileTrackerServer.Repositories.Implementations;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync(Guid userId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.ProductBacklog)
            .Where(p =>
                p.Status == ProjectStatus.Active &&
                p.Members.Any(m => m.UserId == userId)
            )
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid projectId, Guid userId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.ProductBacklog)
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.Status == ProjectStatus.Active &&
                p.Members.Any(m => m.UserId == userId)
            );
    }
    
    

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }
    
    public async Task<IEnumerable<ProjectMember>> GetMembersAsync(
        Guid projectId,
        Guid userId)
    {
        // garante que o usuário faz parte do projeto
        var hasAccess = await _context.ProjectMembers.AnyAsync(pm =>
            pm.ProjectId == projectId &&
            pm.UserId == userId);

        if (!hasAccess)
            throw new DomainException("Você não tem acesso a este projeto.");

        return await _context.ProjectMembers
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .OrderBy(pm => pm.JoinedAt)
            .ToListAsync();
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}