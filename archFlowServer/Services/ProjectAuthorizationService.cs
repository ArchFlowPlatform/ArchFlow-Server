using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class ProjectAuthorizationService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectAuthorizationService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Project> AuthorizeAsync(
        Guid projectId,
        Guid userId,
        params MemberRole[] allowedRoles)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, userId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        if (!project.HasPermission(userId, allowedRoles))
            throw new ForbiddenException("PermissÃ£o insuficiente para acessar este recurso.");

        return project;
    }
}
