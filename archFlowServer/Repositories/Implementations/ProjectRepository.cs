using archFlowServer.Data;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using ArchFlowServer.Models.Dtos.Project;
using Microsoft.EntityFrameworkCore;

namespace archFlowServer.Repositories.Implementations;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Project>> GetAllActiveAsync()
    {
        return await _context.Projects
        .Include(p => p.Members)
            .ThenInclude(pm => pm.User)
        .Where(p => p.Status == ProjectStatus.Active)
        .ToListAsync();
    }

    public async Task<Project?> GetArchivedByIdAsync(Guid projectId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.Status == ProjectStatus.Archived
            );
    }

    public async Task<Project?> GetActiveByIdAsync(Guid projectId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.Status == ProjectStatus.Active
            );
    }
    public async Task<Project?> GetByIdAsync(Guid projectId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.Status == ProjectStatus.Active
            );
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }
    
    public async Task<IReadOnlyList<ProjectMember>> GetMembersAsync(
        Guid projectId)
    {
        // garante que o usuário faz parte do projeto
        var hasAccess = await _context.ProjectMembers.AnyAsync(pm =>
            pm.ProjectId == projectId);

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
