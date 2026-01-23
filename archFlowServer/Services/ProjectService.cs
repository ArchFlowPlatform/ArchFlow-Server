using archFlowServer.Infrastructure.Email;
using archFlowServer.Infrastructure.Email.EmailTemplates;
using archFlowServer.Models.Dtos.Project;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class ProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectInviteRepository _inviteRepository;
    private readonly IEmailService _emailSerivce;
    private readonly IConfiguration _configuration;

    public ProjectService(IProjectRepository repository, IUserRepository userRepository, IProjectInviteRepository inviteRepository, IEmailService emailSerivce,  IConfiguration configuration)
    {
        _repository = repository;
        _userRepository = userRepository;
        _inviteRepository = inviteRepository;
        _emailSerivce = emailSerivce;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync(Guid userId)
    {
        var projects = await _repository.GetAllAsync(userId);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectResponseDto> GetByIdAsync(Guid projectId, Guid userId)
    {
        var project = await _repository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        return MapToDto(project);
    }

    public async Task<ProjectResponseDto> CreateAsync(
        CreateProjectDto dto,
        Guid creatorUserId)
    {
        var project = new Project(
            dto.Name,
            dto.Description,
            creatorUserId
        );

        await _repository.AddAsync(project);
        await _repository.SaveChangesAsync();

        return MapToDto(project);
    }


    public async Task<ProjectResponseDto> UpdateAsync(
        Guid projectId,
        UpdateProjectDto dto,
        Guid executorUserId)
    {
        var project = await _repository.GetByIdAsync(projectId, executorUserId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        project.UpdateDetails(
            executorUserId,
            dto.Name,
            dto.Description
        );

        await _repository.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task ArchiveAsync(Guid projectId, Guid executorUserId)
    {
        var project = await _repository.GetByIdAsync(projectId, executorUserId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        project.Archive(executorUserId);

        await _repository.SaveChangesAsync();
    }
    
    public async Task AddMemberAsync(
        Guid projectId,
        Guid executorUserId,
        Guid newUserId,
        MemberRole role)
    {
        var project = await _repository.GetByIdAsync(projectId, executorUserId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        project.AddMember(executorUserId, newUserId, role);

        await _repository.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(
        Guid projectId,
        Guid executorUserId,
        Guid userId)
    {
        var project = await _repository.GetByIdAsync(projectId, executorUserId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        project.RemoveMember(executorUserId, userId);

        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProjectMemberResponseDto>> GetMembersAsync(
        Guid projectId,
        Guid executorUserId)
    {
        // 1. Valida existÃªncia + acesso ao projeto
        var project = await _repository.GetByIdAsync(projectId, executorUserId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        // 2. Busca membros
        var members = await _repository.GetMembersAsync(
            projectId,
            executorUserId
        );

        // 3. Mapeia para DTO
        return members.Select(m => new ProjectMemberResponseDto
        {
            UserId = m.UserId,
            Name = m.User.Name,
            Email = m.User.Email,
            Role = m.Role,
            JoinedAt = m.JoinedAt
        });
    }
    
    public async Task InviteMemberAsync(
        Guid projectId,
        Guid executorUserId,
        string email,
        MemberRole role)
    {
        // 1) Projeto + permissÃ£o (vocÃª jÃ¡ valida acesso via GetByIdAsync)
        var project = await _repository.GetByIdAsync(projectId, executorUserId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        // 2) Quem convidou (inviter = executorUser)
        var inviter = await _userRepository.GetByIdAsync(executorUserId)
                      ?? throw new NotFoundException("usuário executor não encontrado.");

        // 3) Evita convites duplicados ativos
        if (await _inviteRepository.ExistsActiveInviteAsync(projectId, email))
            throw new ConflictException("JÃ¡ existe um convite ativo para este email.");

        // 4) Cria e persiste (token serÃ¡ utilizado apÃ³s salvar)
        var invite = new ProjectInvite(
            projectId,
            email,
            role,
            TimeSpan.FromDays(3)
        );

        await _inviteRepository.AddAsync(invite);
        await _inviteRepository.SaveChangesAsync();

        // 5) Monta a URL com o token salvo
        var baseUrl = _configuration["App:FrontendBaseUrl"]
                      ?? throw new DomainException("FrontendBaseUrl não configurado.");

        // Exemplo de rota no frontend:
        // /invites/accept?token=XYZ
        var acceptUrl = $"{baseUrl.TrimEnd('/')}/invites/accept?token={invite.Token}";

        // 6) Envia email
        await _emailSerivce.SendAsync(
            invite.Email,
            $"Convite para o projeto {project.Name}",
            ProjectInviteTemplate.Build(
                project.Name,
                inviter.Name,
                acceptUrl
            )
        );
    }
    
    public async Task AcceptInviteAsync(
        string token,
        Guid userId)
    {
        var invite = await _inviteRepository.GetByTokenAsync(token)
                     ?? throw new DomainException("Convite inválido.");

        invite.Accept();

        // adiciona membro real
        var project = await _repository.GetByIdAsync(invite.ProjectId, userId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        var userInvited = await _userRepository.GetByEmailAsync(invite.Email)
                        ?? throw new NotFoundException("usuário não encontrado.");

        project.AddMember(
            executorUserId: userId,
            newUserId: userInvited.Id,
            role: invite.Role
        );
        
        _inviteRepository.Delete(invite);
        await _inviteRepository.SaveChangesAsync();
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
}

