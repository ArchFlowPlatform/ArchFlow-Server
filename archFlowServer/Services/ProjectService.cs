using archFlowServer.Infrastructure.Email;
using archFlowServer.Infrastructure.Email.EmailTemplates;
using archFlowServer.Models.Dtos.Project;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using ArchFlowServer.Models.Dtos.Project;

namespace archFlowServer.Services;

public class ProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectInviteRepository _inviteRepository;
    private readonly IEmailService _emailSerivce;
    private readonly IConfiguration _configuration;

    public ProjectService(
        IProjectRepository repository,
        IUserRepository userRepository,
        IProjectInviteRepository inviteRepository,
        IEmailService emailSerivce,
        IConfiguration configuration)
    {
        _repository = repository;
        _userRepository = userRepository;
        _inviteRepository = inviteRepository;
        _emailSerivce = emailSerivce;
        _configuration = configuration;
    }

    // ==========================
    // Projects
    // ==========================

    public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync(Guid userId)
    {
        var projects = await _repository.GetAllActiveAsync();

        // filtro por membership aqui (repo neutro)
        var visibleProjects = projects.Where(p => p.Members.Any(m => m.UserId == userId));

        return visibleProjects.Select(MapToDto);
    }

    public async Task<ProjectResponseDto> GetByIdAsync(Guid projectId, Guid userId)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, userId);

        return MapToDto(project);
    }

    public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, Guid creatorUserId)
    {
        var project = new Project(dto.Name, dto.Description, creatorUserId);

        await _repository.AddAsync(project);
        await _repository.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task<ProjectResponseDto> UpdateAsync(Guid projectId, UpdateProjectDto dto, Guid executorUserId)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        project.UpdateDetails(executorUserId, dto.Name, dto.Description);

        await _repository.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task ArchiveAsync(Guid projectId, Guid executorUserId)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        project.Archive(executorUserId);

        await _repository.SaveChangesAsync();
    }

    public async Task RestoreAsync(Guid projectId, Guid executorUserId)
    {
        var project = await _repository.GetArchivedByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        project.Restore(executorUserId);

        await _repository.SaveChangesAsync();
    }

    public async Task AddMemberAsync(Guid projectId, Guid executorUserId, Guid newUserId, MemberRole role)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        project.AddMember(executorUserId, newUserId, role);

        await _repository.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid executorUserId, Guid userId)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        project.RemoveMember(executorUserId, userId);

        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProjectMemberResponseDto>> GetMembersAsync(Guid projectId, Guid executorUserId)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        var members = await _repository.GetMembersAsync(projectId);

        return members.Select(m => new ProjectMemberResponseDto
        {
            UserId = m.UserId,
            Name = m.User.Name,
            Email = m.User.Email,
            Role = m.Role,
            JoinedAt = m.JoinedAt
        });
    }

    // ==========================
    // Invites
    // ==========================

    public async Task InviteMemberAsync(Guid projectId, Guid executorUserId, string email, MemberRole role)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, executorUserId);

        var inviter = await _userRepository.GetByIdAsync(executorUserId)
            ?? throw new NotFoundException("usuário executor não encontrado.");

        if (await _inviteRepository.ExistsPendingInviteAsync(projectId, email))
            throw new ConflictException("Já existe um convite pendente para este email.");

        var invite = new ProjectInvite(
            projectId,
            email,
            role,
            TimeSpan.FromDays(3));

        await _inviteRepository.AddAsync(invite);
        await _inviteRepository.SaveChangesAsync();

        var baseUrl = _configuration["App:FrontendBaseUrl"]
            ?? throw new DomainException("FrontendBaseUrl não configurado.");

        var acceptUrl = $"{baseUrl.TrimEnd('/')}/invites/accept?token={invite.Token}";

        await _emailSerivce.SendAsync(
            invite.Email,
            $"Convite para o projeto {project.Name}",
            ProjectInviteTemplate.Build(project.Name, inviter.Name, acceptUrl));
    }

    public async Task<IEnumerable<ProjectInviteResponseDto>> GetAllProjectsInviteAsync(Guid projectId, Guid userId)
    {
        var project = await _repository.GetActiveByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        EnsureMember(project, userId);

        var invites = await _inviteRepository.GetAllAsync(projectId) ?? Enumerable.Empty<ProjectInvite>();

        return invites.Select(MapInviteToDto);
    }

    public async Task AcceptInviteAsync(string token, Guid userId)
    {
        var invite = await _inviteRepository.GetByTokenAsync(token)
            ?? throw new DomainException("Convite inválido.");

        var project = await _repository.GetByIdAsync(invite.ProjectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var userInvited = await _userRepository.GetByEmailAsync(invite.Email)
            ?? throw new NotFoundException("usuário não encontrado.");

        invite.Accept();

        project.AddMember(
            executorUserId: userId,
            newUserId: userInvited.Id,
            role: invite.Role);

        await _inviteRepository.SaveChangesAsync();
    }

    public async Task DeclineInviteAsync(string token)
    {
        var invite = await _inviteRepository.GetByTokenAsync(token)
            ?? throw new DomainException("Convite inválido.");

        invite.Decline();

        await _inviteRepository.SaveChangesAsync();
    }

    public async Task RevokeInviteAsync(string token)
    {
        var invite = await _inviteRepository.GetByTokenAsync(token)
            ?? throw new DomainException("Convite inválido.");

        invite.Revoke();

        await _inviteRepository.SaveChangesAsync();
    }

    // ==========================
    // Guards / Mappers
    // ==========================

    private static void EnsureMember(Project project, Guid userId)
    {
        if (!project.Members.Any(m => m.UserId == userId))
            throw new ForbiddenException("Você não tem acesso a este projeto.");
    }

    private static ProjectResponseDto MapToDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CreatedAt = project.CreatedAt
        };
    }

    private static ProjectInviteResponseDto MapInviteToDto(ProjectInvite projectInvite)
    {
        return new ProjectInviteResponseDto
        {
            Id = projectInvite.Id,
            ProjectId = projectInvite.ProjectId,
            Email = projectInvite.Email,
            Role = projectInvite.Role,
            Token = projectInvite.Token,
            ExpiresAt = projectInvite.ExpiresAt,
            CreatedAt = projectInvite.CreatedAt,
            Status = projectInvite.Status
        };
    }
}
